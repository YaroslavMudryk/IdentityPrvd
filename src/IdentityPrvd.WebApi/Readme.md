# IdentityPrvd Service API

#Endpoints

Basic path

## Register
```
POST /api/identity/signup
```

---

## Login
```
POST /api/identity/signin
```

## Refresh-Token
```
POST /api/identity/refresh-token
```

---

## Change Login
```
POST /api/identity/change-login
```
## Change Password
```
POST /api/identity/change-password
```
## Forgot Password
```
POST /api/identity/forgot-password
```

---

## Get Sessions
```
GET /api/identity/sessions
```
## Revoke Sessions
```
DELETE /api/identity/revoke-sessions
```

---

## Link external services
## Login by external services

---

## Enable mfa
```
POST /api/identity/mfa
```
## Disable mfa
```
DELETE /api/identity/mfa
```

---

## Logout
```
DELETE /api/identity/logout
```

---

## Roles
### Get roles
```
GET /api/identity/roles
```
### Create roles
```
POST /api/identity/roles
```
### Edit roles
```
PUT /api/identity/roles/{roleId}
```
### Delete roles
```
DELETE /api/identity/roles/{roleId}
```

---

## Claims
### Get claims
```
GET /api/identity/claims
```
### Create claims
```
POST /api/identity/claims
```
### Edit claims
```
PUT /api/identity/claims/{claimId}
```
### Delete claims
```
DELETE /api/identity/claims
```

---

## Clients
### Get clients
```
GET /api/identity/clients
```
### Create clients
```
POST /api/identity/clients
```
### Edit clients
```
PUT /api/identity/clients/{clientId}
```
### Delete clients
```
DELETE /api/identity/clients/{clientId}
```