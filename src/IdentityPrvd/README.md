# IdentityPrvd

Комплексна бібліотека для побудови identity‑провайдера на .NET. Нижче підсумовано всі публічні ендпоінти: що приймають, що віддають, як взаємодіють між собою та які ключові частини реалізації задіяні.

## Архітектура API
- Мінімальні ендпоінти (`IEndpoint`) реєструються через `EndpointsExtensions.AddEndpoints()` і мапляться в `MapEndpoints()`.
- Пайплайн поділено на чотири домени: **Authentication**, **Authorization**, **Personal**, **Security**.
- DTO валідовуються через FluentValidation (`ValidationHelper.ValidateAndThrowAsync`), відповіді обгортаються в `ApiResponse.MapToResponse()`.

## Аутентифікація

### Базові сценарії входу
#### POST `/api/identity/signin`
- **Призначення:** класичний логін за логіном/паролем.
- **Приймає:** `SigninRequestDto` (`login`, `password`, `language`, `clientId`, `clientSecret`, `appVersion`, довільні `data`, `client` про пристрій).
- **Повертає:** `SigninResponseDto` (JWT, refresh, час життя, ознака `requiredMfa`, `verifyId` коли MFA обов’язкове).
- **Пов'язаний з:** `signin-mfa`, `signin-user-options`, `refresh-token`, менеджментом сесій.
- **Реалізація:** `SigninOrchestrator` (`Features/Authentication/Signin/Services/SigninOrchestrator.cs`) перевіряє клієнта, резолвить геодані через `ILocationService`, створює `IdentitySession` і refresh‑токен, пушить сесію у кеш (`ISessionManager`) та закриває інші при необхідності через `ISessionControlService`. Якщо для юзера активовано MFA — створює `SessionType.Mfa` зі `VerificationId`.

#### POST `/api/identity/signin-mfa`
- **Призначення:** підтвердження MFA після попереднього логіну.
- **Приймає:** `SigninMfaRequestDto` (`verificationId`, `code`).
- **Повертає:** `SigninResponseDto` з бойовими токенами.
- **Пов'язаний з:** попереднім `signin`, `enable/disable mfa`, `refresh-token`.
- **Реалізація:** `SigninMfaOrchestrator` знаходить сесію за `verificationId`, перевіряє TOTP через `IMfaService`, переводить її в `SessionStatus.Active`, випускає новий refresh, генерує JWT та оновлює кеш сесій.

#### POST `/api/identity/signin-passwordless`
- **Призначення:** OTP‑вхід без пароля на основі активованої MFA.
- **Приймає:** `PasswordlessSigninRequestDto` (логін, OTP‑код, `clientId/secret`, `appVersion`, `client`, `data`).
- **Повертає:** `SigninResponseDto`.
- **Пов'язаний з:** `enable mfa`, `signin-mfa`, `refresh-token`.
- **Реалізація:** `PasswordlessSigninOrchestrator` підтверджує, що MFA активна, верифікує код, створює `SessionType.Mfa`, refresh‑токен, обчислює дозволи для клієнта і закриває зайві сесії.

#### GET `/api/identity/signin-user-options`
- **Призначення:** дізнатися, які механізми входу доступні конкретному логіну.
- **Приймає:** query `login`.
- **Повертає:** `SigninUserOptionsDto` (`password`, `passwordless`, `linkedExternalProviders`).
- **Пов'язаний з:** UI вибору методу входу, `linked-external-signin`.
- **Реалізація:** `SigninOptionsOrchestrator` шукає користувача, перевіряє наявність пароля (`PasswordHash`), активну MFA (`IMfaStore`), витягує зовнішні прив’язки (`IUserLoginsQuery`). Для неіснуючих логінів повертає порожні опції щоб не «палити» користувачів.

#### GET `/api/identity/signin-options`
- **Призначення:** глобальна конфігурація доступних способів входу.
- **Повертає:** `SigninOptionsDto` (`password`, `passwordless`, список зовнішніх провайдерів).
- **Реалізація:** `Features/Authentication/SigninOptions/Services/SigninOptionsOrchestrator` комбінує `IdentityPrvdOptions.Signin` та зареєстровані `IAuthSchemes`.

#### POST `/api/identity/signout`
- **Призначення:** вихід з поточної сесії або з усіх.
- **Приймає:** query `everywhere` (bool, за замовчуванням `false`).
- **Повертає:** `204 No Content`.
- **Пов'язаний з:** `sessions` та `revoke-sessions`.
- **Реалізація:** `SignoutOrchestrator` (через `ISessionControlService`) або закриває одну сесію, або всі активні користувача, валідує права через `IdentityClaims`.

### Реєстрація та підтвердження
#### POST `/api/identity/signup`
- **Приймає:** `SignupRequestDto` (ім’я, логін, пароль, профільні поля).
- **Повертає:** `SignupResponseDto` (ID, логін, юзернейм).
- **Пов'язаний з:** `signup/confirm`, `start-restore-password` (код повторно використовує ті ж механізми).
- **Реалізація:** `SignupOrchestrator` створює `IdentityUser`, прив’язує дефолтну роль (`IRolesQuery.GetDefaultRoleIdAsync`), пароль, за потреби додає контакт, надсилає код підтвердження через email/SMS якщо `options.User.ConfirmRequired`.

#### POST `/api/identity/signup/confirm`
- **Приймає:** `SignupConfirmRequestDto` (`code`).
- **Повертає:** `204`.
- **Пов'язаний з:** попередній `signup`.
- **Реалізація:** `SignupConfirmOrchestrator` шукає `IdentityCode`, активує його, виставляє прапор `IsConfirmed` у користувача.

### Керування обліковими даними
#### POST `/api/identity/change-login`
- **Приймає:** `ChangeLoginDto` (`newLogin`, `password` опційно).
- **Повертає:** `204`.
- **Пов'язаний з:** політиками `IdentityPrvdOptions.User` (тип логіна, вимога паролю), перевірками зайнятості логіна.
- **Реалізація:** `ChangeLoginOrchestrator` валідує формат (email/phone/any), опційно перевіряє пароль, гарантує унікальність через `IUsersQuery`.

#### POST `/api/identity/change-password`
- **Приймає:** `ChangePasswordDto` (старий/новий пароль, hint, `signoutEverywhere`).
- **Повертає:** `204`.
- **Пов'язаний з:** `signout`, `refresh-token` (всі токени інвалідовано).
- **Реалізація:** `ChangePasswordOrchestrator` перевіряє старий пароль, політику reuse (`options.User.UseOldPasswords`), деактивує попередні записи в `IdentityPassword`, додає новий, за потреби закриває всі сесії й refresh‑токени.

#### POST `/api/identity/start-restore-password`
- **Приймає:** `StartRestorePasswordDto` (`login`).
- **Повертає:** `StartedRestorePasswordDto` (`login`, `verifyId` – код не повертається, надсилається каналом логіна).
- **Пов'язаний з:** наступний `/restore-password`.
- **Реалізація:** `StartRestorePasswordOrchestrator` перевіряє, що користувач існує, генерує `IdentityCode` (hash коду), зберігає `verifyId`, надсилає код через email/SMS.

#### POST `/api/identity/restore-password`
- **Приймає:** `RestorePasswordDto` (`hint`, `password`, отримані `code`, `verifyId`).
- **Повертає:** `204`.
- **Пов'язаний з:** `start-restore-password`, `password` історією.
- **Реалізація:** `RestorePasswordOrchestrator` валідовує запит, перевіряє, що `IdentityCode` ще активний і код співпадає (через `IHasher.Verify`), деактивує старі паролі, обновлює `IdentityUser.PasswordHash`, створює новий запис в `IdentityPassword`.

### Зовнішні провайдери та SSO
#### GET `/api/identity/signin-external`
- **Приймає:** query з `ExternalSigninDto` (провайдер, `returnUrl`, client/device/os/browser метадані).
- **Повертає:** HTTP 401 Challenge на потрібну схему.
- **Пов'язаний з:** `signin-external-callback`, `link-external-signin`.
- **Реалізація:** `ExternalSigninEndpoint` з FluentValidation, формує `AuthenticationProperties` з RedirectUri (`SigninExternalCallback`) та кладe DTO у `authProperties.Items`.

#### GET `/api/identity/signin-external-callback`
- **Приймає:** `returnUrl`, `provider`.
- **Повертає:** redirect на `returnUrl` з query (`accessToken`, `refreshToken`, `expireIn`).
- **Пов'язаний з:** попередній крок, `ExternalSigninOrchestrator`.
- **Реалізація:** автентифікує користувача через `ExternalProviderManager`, викликає `ExternalSigninOrchestrator`, який:
  - екстрактить профіль (`ExternalUserExtractorService`),
  - створює юзера+роль+логін за потреби,
  - створює сесію (`IdentitySession`) + refresh,
  - видає токени та синхронізує `ISessionManager`.

#### GET `/api/identity/linked-external-signin`
- **Повертає:** список `ExternalProviderDto` (провайдер, картинка, чи лінковано, коли).
- **Пов'язаний з:** UI керування провайдерами, `link`/`unlink`.
- **Реалізація:** `LinkedExternalSigninOrchestrator` бере поточного користувача, перетинає його логіни з усіма схемами `IAuthSchemes`.

#### GET `/api/identity/link-external-signin`
- **Приймає:** query `provider`, `returnUrl`.
- **Повертає:** 401 Challenge з Redirect на `LinkSigninExternalCallback`.
- **Реалізація:** Дозволений лише для автентифікованих. В `AuthenticationProperties.Items` пише `CurrentUserId` для подальшої обробки.

#### GET `/api/identity/link-external-signin-callback`
- **Повертає:** redirect на `returnUrl?status=link_success`.
- **Реалізація:** `LinkExternalSigninOrchestrator` перевіряє `AuthenticateResult`, створює запис в `IdentityUserLogin`, не дозволяє повторної прив’язки.

#### DELETE `/api/identity/unlink-external-signin`
- **Приймає:** query `provider`.
- **Повертає:** повідомлення про результат.
- **Реалізація:** `UnlinkExternalSigninOrchestrator` видаляє запис `IdentityUserLogin`.

#### GET `/api/identity/sso`
- **Приймає:** (опціонально) `accessToken`.
- **Повертає:** список claims поточного користувача.
- **Пов'язаний з:** редіректи зовнішніх провайдерів і SPA, дефолтне `returnUrl`.
- **Реалізація:** `DefaultReturnUriEndpoint` якщо користувач не автентифікований, але передано токен — створює `ClaimsPrincipal` через `JwtPrincipalFactory`.

### QR-вхід
#### GET `/auth/qr`
- **Призначення:** WebSocket канал для push‑сповіщень під час QR логіну.
- **Приймає:** `verificationId` у query; запит мусить бути WebSocket.
- **Відповідь:** апгрейд зберігається до закриття; немає payload.
- **Реалізація:** `QrSigninEndpoint` керує `IWebSocketConnectionManager`, знімає сокет, слухає до закриття.

#### POST `/api/identity/qr`
- **Приймає:** `QrRequestDto` (`clientId/secret`, `appVersion`, `language`, довільні `data`, `client`).
- **Повертає:** `QrCodeDto` (verificationId, base64 PNG).
- **Реалізація:** `QrCodeService.GenerateQrCodeAsync` створює запис у `IWebSocketConnectionManager`, генерує QR через `QRCoder`.

#### GET `/api/identity/qr/{verificationId}`
- **Повертає:** `ClientInfo` збережений для QR‑запиту.
- **Реалізація:** через `IQrCodeService.GetQrCodeDetailsAsync` лише для автентифікованих (щоб бачити, що авторизовують).

#### POST `/api/identity/qr/confirm`
- **Приймає:** `QrConfirmDto` (`verificationId` в body).
- **Повертає:** `ConfirmQrDto` (`ok`, `error`), а у фоновому режимі відправляє `SigninResponseDto` по WebSocket на сторону, що сканувала.
- **Реалізація:** `ConfirmQrCodeAsync` створює сесію від імені поточного користувача для клієнта зі сканера, видає токени, пушить JSON у сокет і зачиняє його.

## Авторизація

### Керування клеймами
> DTO: `ClaimDto`, `CreateClaimDto`, `UpdateClaimDto`.

#### GET `/api/identity/claims`
- **Повертає:** перелік клеймів з метаданими (лічильники ролей/клієнтів).
- **Реалізація:** `GetClaimsOrchestrator` читає з `IClaimsQuery`.

#### POST `/api/identity/claims`
- **Приймає:** тип, значення, issuer, displayName.
- **Повертає:** створений `ClaimDto` (201).
- **Реалізація:** `CreateClaimOrchestrator` додає через `IClaimStore`, відповідає JSON.

#### PUT `/api/identity/claims/{claimId}`
- **Приймає:** `UpdateClaimDto`.
- **Повертає:** оновлений `ClaimDto`.
- **Реалізація:** `UpdateClaimOrchestrator` валідує наявність, дозволяє змінювати метадані.

#### DELETE `/api/identity/claims/{claimId}`
- **Повертає:** `204`.
- **Реалізація:** `DeleteClaimOrchestrator` видаляє клейм; при невдачі кине `NotFoundException`.

### Ролі
> DTO: `RoleDto`, `CreateRoleDto`, `UpdateRoleDto`.

#### GET `/api/identity/roles`
- **Призначення:** список ролей з кількістю користувачів/клейм.
- **Реалізація:** `GetRolesOrchestrator`.

#### POST `/api/identity/roles`
- **Приймає:** назву, прапорець `isDefault`, масив `claimIds`.
- **Повертає:** створену роль (201).
- **Реалізація:** `CreateRoleOrchestrator` створює роль та прив’язує клейми.

#### PUT `/api/identity/roles/{roleId}`
- **Приймає:** `UpdateRoleDto`.
- **Повертає:** актуалізовану роль.
- **Реалізація:** `UpdateRoleOrchestrator` синхронізує назву, дефолтність та клейми.

#### DELETE `/api/identity/roles/{roleId}`
- **Повертає:** `204`.
- **Реалізація:** `DeleteRoleOrchestrator` перевіряє наявність та видаляє.

### Клієнти (OAuth apps / SPA)
> DTO: `ClientDto`, `CreateClientDto`, `UpdateClientDto`, `UpdateClientClaimsDto`.

#### GET `/api/identity/clients`
- **Повертає:** список клієнтів з redirect URI, описом, активністю.
- **Реалізація:** `GetClientsOrchestrator`.

#### GET `/api/identity/clients/{clientId}`
- **Повертає:** конкретний клієнт.
- **Реалізація:** `GetClientOrchestrator`.

#### POST `/api/identity/clients`
- **Приймає:** назву, `clientId`, налаштування секрету, redirect URI, період активності, опис/зображення.
- **Повертає:** створений клієнт (201) разом зі згенерованим `ClientSecret`.
- **Реалізація:** `CreateClientOrchestrator`.

#### PUT `/api/identity/clients/{clientId}`
- **Приймає:** `UpdateClientDto`.
- **Повертає:** оновлений клієнт.
- **Реалізація:** `UpdateClientOrchestrator`.

#### PUT `/api/identity/clients/{clientId}/claims`
- **Приймає:** `UpdateClientClaimsDto` (список ID клейм).
- **Повертає:** клієнт з актуальним набором клеймів.
- **Реалізація:** `UpdateClientClaimsOrchestrator` оновлює зв’язки в `IClientClaimStore`.

#### DELETE `/api/identity/clients/{clientId}`
- **Повертає:** `204`.
- **Реалізація:** `DeleteClientOrchestrator`.

## Особисті дані користувача

### Контакти
> DTO: `ContactDto`, `CreateContactDto`.

#### GET `/api/identity/contacts`
- **Повертає:** контакти користувача з типом (`ContactType`), ознакою `IsMain`.
- **Реалізація:** `GetContactsOrchestrator`.

#### POST `/api/identity/contacts`
- **Приймає:** назву, значення, тип.
- **Повертає:** створений контакт.
- **Реалізація:** `CreateContactOrchestrator` додає `IdentityContact`, валідує що користувач може додавати.

#### DELETE `/api/identity/contacts/{id}`
- **Повертає:** `204`.
- **Реалізація:** `DeleteContactOrchestrator`.

### Пристрої
> DTO: `DeviceDto`, `VerifyDeviceDto`.

#### GET `/api/identity/devices`
- **Повертає:** список пристроїв з характеристиками ОС/браузера та статусом верифікації.
- **Реалізація:** `GetDevicesOrchestrator`.

#### POST `/api/identity/devices/verify`
- **Приймає:** `VerifyDeviceDto` (ідентифікатор, бренд, модель, ОС, браузер).
- **Повертає:** `DeviceDto`.
- **Реалізація:** `VerifyDeviceOrchestrator` або створює, або оновлює пристрій, позначає як верифікований.

#### POST `/api/identity/devices/unverify/{deviceId}`
- **Повертає:** `204`.
- **Реалізація:** `UnverifyDeviceOrchestrator` скидає прапор перевірки.

#### DELETE `/api/identity/devices/{deviceId}`
- **Повертає:** `204`.
- **Реалізація:** `DeleteDeviceOrchestrator`.

## Безпека та сесії

### Ініціалізація системи
#### POST `/api/system/initialize`
- **Приймає:** `InitializeRequestDto` (`appVersion`).
- **Повертає:** `InitializeResponseDto` (одноразовий логін/пароль адміністратора + токени).
- **Реалізація:** `InitializeOrchestrator` перевіряє статус (`ISystemStatus`), створює базу, сипле сидові ролі/клейми/клієнти (`SeedConstants`), генерує системного користувача та активну сесію з refresh‑токеном і повертає доступ.

### MFA
#### POST `/api/identity/mfa`
- **Приймає:** `MfaDto` з `totp` (може бути порожнім).
- **Повертає:** або `204`, або `MfaResponse` (URL для сканування, секрет, recovery‑коди).
- **Пов'язаний з:** `signin-mfa`, `passwordless`.
- **Реалізація:** `EnableMfaOrchestrator` без `totp` генерує секрет, QR (`OtpUri`) і recovery‑коди (`IdentityMfaRecoveryCode`), з `totp` — перевіряє код і активує MFA.

#### DELETE `/api/identity/mfa`
- **Приймає:** query/body `code`.
- **Повертає:** `204`.
- **Реалізація:** `DisableMfaOrchestrator` перевіряє код через `IMfaService`, видаляє MFA та recovery‑коди.

### Refresh токен
#### POST `/api/identity/refresh-token`
- **Приймає:** `RefreshTokenDto` (`token`).
- **Повертає:** новий `SigninResponseDto`.
- **Реалізація:** `RefreshTokenOrchestrator` валідовує токен, позначає старий як використаний (`UsedAt`), створює новий refresh, перевидає JWT через `ITokenService`.

### Сесії
#### GET `/api/identity/sessions`
- **Повертає:** масив `SessionDto` (app, клієнт, статус, остання активність).
- **Реалізація:** `GetSessionsOrchestrator` об’єднує БД (`ISessionsQuery`) та кеш `ISessionManager`, сортує так, щоб поточна сесія була зверху.

#### GET `/api/identity/sessions/{sessionId}`
- **Повертає:** `SessionDetailDto` з повним описом (локація, тип, `viaMfa`, пов’язані пристрої).
- **Реалізація:** `GetSessionOrchestrator` валідує, що сесія належить користувачу, додає `LastActivityAt` з кешу.

#### DELETE `/api/identity/revoke-sessions`
- **Приймає:** масив рядків `sessionIds` в тілі.
- **Повертає:** кількість закритих сесій (Ok + цифра).
- **Реалізація:** `RevokeSessionsOrchestrator` перетворює на GUID, через `SessionRevocationValidator` перевіряє доступність, виставляє `SessionStatus.Close`, оновлює refresh‑токени та чистить кеш (`ISessionManager.DeleteSessionsByIdsAsync`).

## Взаємозв’язки
- `Signin` → `SigninMfa` → `RefreshToken` → `Sessions` → `Revoke/Signout`.
- `Enable/DisableMfa` впливають на `signin-passwordless` та на те, що повертає `signin-user-options`.
- `Signup` + `SignupConfirm` + `Start/Restore password` спільно використовують `IdentityCode`.
- `External signin` і `Link external signin` дзеркалять один одного: перший створює або знаходить користувача, другий лише додає провайдера для існуючого.
- `QR` сценарій використовує ті ж сервіси, що і класичний логін: створення сесії, токени, закриття старих сесій.
- Адміністративні частини (`claims`, `roles`, `clients`) безпосередньо впливають на те, які клейми видає `ITokenService`, а отже й на авторизацію у всіх інших ендпоінтах.

## Де шукати код
- Ендпоінти: `src/IdentityPrvd/Features/**/...Endpoint.cs`.
- DTO + валідація: відповідні `Dtos` та `Dtos/Validators`.
- Оркестратори/бізнес-логіка: `Features/**/Services`.
- Спільні респонси: `IdentityPrvd.Common.Api` та `Features.Shared.Dtos`.
