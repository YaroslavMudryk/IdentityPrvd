## Test status

This document tracks the testing status of the IdentityPrvd library endpoints.

- **Implemented**: The endpoint is coded and wired.
- **Manually tested**: Verified manually (e.g., via Postman, curl, browser).
- **Has tests**: Automated integration/unit tests exist and pass.

Use the checkboxes to mark progress.

## Endpoints

| Group | Method | Path | Implemented | Manually tested | Has tests |
|---|---|---|---|---|---|
| Authentication | POST | `/api/identity/signin` | [✅] | [✅] | [❌] |
| Authentication | POST | `/api/identity/signin-mfa` | [✅] | [✅] | [❌] |
| Authentication | POST | `/api/identity/signout` | [✅] | [✅] | [❌] |
| Authentication | GET | `/api/identity/signin-options` | [✅] | [✅] | [❌] |
| Authentication | POST | `/api/identity/signup` | [✅] | [✅] | [❌] |
| Authentication | POST | `/api/identity/signup/confirm` | [✅] | [✅] | [❌] |
| Authentication | GET | `/api/identity/signin-external` | [✅] | [✅] | [❌] |
| Authentication | GET | `/api/identity/signin-external-callback` | [✅] | [✅] | [❌] |
| Authentication | GET | `/api/identity/linked-external-signin` | [✅] | [✅] | [❌] |
| Authentication | GET | `/api/identity/link-external-signin` | [✅] | [✅] | [❌] |
| Authentication | GET | `/api/identity/link-external-signin-callback` | [✅] | [✅] | [❌] |
| Authentication | DELETE | `/api/identity/unlink-external-signin` | [✅] | [✅] | [❌] |
| Authentication | GET | `/api/identity/sso` | [✅] | [✅] | [❌] |
| QR Signin | GET (WebSocket) | `/auth` | [✅] | [✅❌] | [❌] |
| QR Signin | POST | `/api/identity/qr` | [✅] | [✅] | [❌] |
| QR Signin | GET | `/api/identity/qr/{verificationId}` | [✅] | [✅] | [❌] |
| QR Signin | POST | `/api/identity/qr/confirm` | [✅] | [✅] | [❌] |
| Account | POST | `/api/identity/change-login` | [✅] | [✅] | [❌] |
| Account | POST | `/api/identity/change-password` | [✅] | [✅] | [❌] |
| Password restore | POST | `/api/identity/start-restore-password` | [✅] | [✅] | [❌] |
| Password restore | POST | `/api/identity/restore-password` | [✅] | [✅] | [❌] |
| Security | POST | `/api/identity/refresh-token` | [✅] | [✅] | [❌] |
| Security | GET | `/api/identity/sessions` | [✅] | [✅] | [❌] |
| Security | DELETE | `/api/identity/revoke-sessions` | [✅] | [✅] | [❌] |
| Security (MFA) | POST | `/api/identity/mfa` | [✅] | [✅] | [❌] |
| Security (MFA) | DELETE | `/api/identity/mfa` | [✅] | [✅] | [❌] |
| Personal (Contacts) | GET | `/api/identity/contacts` | [✅] | [✅] | [❌] |
| Personal (Contacts) | POST | `/api/identity/contacts` | [✅] | [❌] | [❌] |
| Personal (Contacts) | DELETE | `/api/identity/contacts/{id}` | [✅] | [❌] | [❌] |
| Personal (Devices) | GET | `/api/identity/devices` | [✅] | [✅] | [❌] |
| Personal (Devices) | POST | `/api/identity/devices/verify` | [✅] | [❌] | [❌] |
| Personal (Devices) | DELETE | `/api/identity/devices/unverify/{deviceId}` | [✅] | [❌] | [❌] |
| Authorization (Claims) | GET | `/api/identity/claims` | [✅] | [❌] | [❌] |
| Authorization (Claims) | POST | `/api/identity/claims` | [✅] | [❌] | [❌] |
| Authorization (Claims) | PUT | `/api/identity/claims/{claimId}` | [✅] | [❌] | [❌] |
| Authorization (Claims) | DELETE | `/api/identity/claims/{claimId}` | [✅] | [❌] | [❌] |
| Authorization (Roles) | GET | `/api/identity/roles` | [✅] | [❌] | [❌] |
| Authorization (Roles) | POST | `/api/identity/roles` | [✅] | [❌] | [❌] |
| Authorization (Roles) | PUT | `/api/identity/roles/{roleId}` | [✅] | [❌] | [❌] |
| Authorization (Roles) | DELETE | `/api/identity/roles/{roleId}` | [✅] | [❌] | [❌] |
