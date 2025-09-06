## Test status

This document tracks the testing status of the IdentityPrvd library endpoints.

- **Implemented**: The endpoint is coded and wired.
- **Manually tested**: Verified manually (e.g., via Postman, curl, browser).
- **Has tests**: Automated integration/unit tests exist and pass.

Use the checkboxes to mark progress.

## Endpoints

### Authentication

| Method | Path | Implemented | Manually tested | Has tests |
|---|---|---|---|---|
| POST | `/api/identity/signin` | [✅] | [✅] | [❌] |
| POST | `/api/identity/signin-mfa` | [✅] | [❌] | [❌] |
| POST | `/api/identity/signout` | [✅] | [✅] | [❌] |
| GET | `/api/identity/signin-options` | [✅] | [✅] | [❌] |
| POST | `/api/identity/signup` | [✅] | [✅] | [❌] |
| POST | `/api/identity/signup/confirm` | [✅] | [✅] | [❌] |
| GET | `/api/identity/signin-external` | [✅] | [❌] | [❌] |
| GET | `/api/identity/signin-external-callback` | [✅] | [❌] | [❌] |
| GET | `/api/identity/linked-external-signin` | [✅] | [❌] | [❌] |
| GET | `/api/identity/link-external-signin` | [✅] | [❌] | [❌] |
| GET | `/api/identity/link-external-signin-callback` | [✅] | [❌] | [❌] |
| DELETE | `/api/identity/unlink-external-signin` | [✅] | [❌] | [❌] |
| GET | `/api/identity/sso` | [✅] | [✅] | [❌] |

#### QR Signin

| Method | Path | Implemented | Manually tested | Has tests |
|---|---|---|---|---|
| GET (WebSocket) | `/auth` | [✅] | [❌] | [❌] |
| POST | `/api/identity/qr` | [✅] | [❌] | [❌] |
| GET | `/api/identity/qr/{verificationId}` | [✅] | [❌] | [❌] |
| POST | `/api/identity/qr/confirm` | [✅] | [❌] | [❌] |

#### Account changes

| Method | Path | Implemented | Manually tested | Has tests |
|---|---|---|---|---|
| POST | `/api/identity/change-login` | [✅] | [❌] | [❌] |
| POST | `/api/identity/change-password` | [✅] | [❌] | [❌] |

#### Password restore

| Method | Path | Implemented | Manually tested | Has tests |
|---|---|---|---|---|
| POST | `/api/identity/start-restore-password` | [✅] | [❌] | [❌] |
| POST | `/api/identity/restore-password` | [✅] | [❌] | [❌] |

### Security

| Method | Path | Implemented | Manually tested | Has tests |
|---|---|---|---|---|
| POST | `/api/identity/refresh-token` | [✅] | [❌] | [❌] |
| GET | `/api/identity/sessions` | [✅] | [✅] | [❌] |
| DELETE | `/api/identity/revoke-sessions` | [✅] | [❌] | [❌] |

#### MFA

| Method | Path | Implemented | Manually tested | Has tests |
|---|---|---|---|---|
| POST | `/api/identity/mfa` | [✅] | [❌] | [❌] |
| DELETE | `/api/identity/mfa` | [✅] | [❌] | [❌] |

### Personal

#### Contacts

| Method | Path | Implemented | Manually tested | Has tests |
|---|---|---|---|---|
| GET | `/api/identity/contacts` | [✅] | [❌] | [❌] |
| POST | `/api/identity/contacts` | [✅] | [❌] | [❌] |
| DELETE | `/api/identity/contacts/{id}` | [✅] | [❌] | [❌] |

#### Devices

| Method | Path | Implemented | Manually tested | Has tests |
|---|---|---|---|---|
| GET | `/api/identity/devices` | [✅] | [❌] | [❌] |
| POST | `/api/identity/devices/verify` | [✅] | [❌] | [❌] |
| DELETE | `/api/identity/devices/unverify/{deviceId}` | [✅] | [❌] | [❌] |

### Authorization

#### Claims

| Method | Path | Implemented | Manually tested | Has tests |
|---|---|---|---|---|
| GET | `/api/identity/claims` | [✅] | [❌] | [❌] |
| POST | `/api/identity/claims` | [✅] | [❌] | [❌] |
| PUT | `/api/identity/claims/{claimId}` | [✅] | [❌] | [❌] |
| DELETE | `/api/identity/claims/{claimId}` | [✅] | [❌] | [❌] |

#### Roles

| Method | Path | Implemented | Manually tested | Has tests |
|---|---|---|---|---|
| GET | `/api/identity/roles` | [✅] | [❌] | [❌] |
| POST | `/api/identity/roles` | [✅] | [❌] | [❌] |
| PUT | `/api/identity/roles/{roleId}` | [✅] | [❌] | [❌] |
| DELETE | `/api/identity/roles/{roleId}` | [✅] | [❌] | [❌] |


