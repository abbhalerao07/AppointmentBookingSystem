## Project Overview

This project is about **Appointment Booking System**

This solution follows a **microservices architecture** and is divided into **4 main APIs** and **1 background worker**:

- **Appointment.API**
- **Availability.API**
- **Identity.API**
- **Schedule.API**
- **Notification.Worker**

The system allows users to:

- register and log in
- receive JWT-based authentication
- receive and use refresh tokens
- renew expired access tokens securely
- view doctors and schedules
- check available slots
- book and cancel appointments
- trigger notifications through a background worker

---

## Architecture Summary

### Services

#### 1. Identity.API
Responsible for authentication and authorization.

**Responsibilities:**
- user registration
- user login
- JWT access token generation
- refresh token generation
- refresh token renewal flow
- token validation configuration support for protected APIs

#### 2. Appointment.API
Handles appointment-related operations.

**Responsibilities:**
- booking appointments
- viewing appointments
- cancelling appointments
- coordinating with other services for doctor and slot data

#### 3. Availability.API
Manages appointment slot availability.

**Responsibilities:**
- checking available time slots
- reserving booked slots
- releasing cancelled slots
- maintaining slot state

#### 4. Schedule.API
Manages doctor and schedule-related data.

**Responsibilities:**
- doctor listing
- doctor schedules
- time slot and schedule management

#### 5. Notification.Worker
Background worker for asynchronous notification processing.

**Responsibilities:**
- consuming appointment events
- processing booking/cancellation notifications
- decoupling notifications from API requests

---

## High-Level Flow

1. User authenticates through **Identity.API**
2. Identity service returns an **access token** and **refresh token**
3. Client uses the access token to call protected APIs
4. Protected APIs validate the JWT locally using configured authentication middleware and `[Authorize]`
5. When the access token expires, the client sends the refresh token to **Identity.API**
6. **Identity.API** issues a new access token (and refresh token if rotation is implemented)
7. **Appointment.API** coordinates with:
   - **Schedule.API** to get doctor/schedule information
   - **Availability.API** to confirm and reserve slots
8. Appointment events are processed by **Notification.Worker**

---

## Authentication and Authorization

This project uses **JWT Bearer Authentication with Refresh Tokens**.

### Authentication flow
- user logs in through `Identity.API`
- `Identity.API` issues:
  - an **access token**
  - a **refresh token**
- the access token is sent in request headers:

`Authorization: Bearer <access-token>`

### Authorization in services
Protected APIs use `[Authorize]`.

That means:
- `Identity.API` issues the access token
- the other APIs validate the token locally using JWT bearer authentication
- they do **not** call `Identity.API` for every authorized request
- they trust the token if the issuer, audience, signature, and expiry are valid

### Refresh token flow
- when the access token expires, the client sends the refresh token to `Identity.API`
- `Identity.API` validates the refresh token
- if valid, a new access token is issued
- depending on implementation, a new refresh token may also be issued

---

## Main Features

- user registration and login
- JWT-based authentication
- refresh token support
- protected API endpoints using `[Authorize]`
- doctor and schedule retrieval
- slot availability management
- appointment booking
- appointment cancellation
- asynchronous notification handling
- separation of concerns through microservices

---

## Security Notes

- protected endpoints use `[Authorize]`
- JWT access tokens are validated locally by each protected API
- refresh tokens are used to obtain new access tokens after expiry
- secrets should be stored securely, not hardcoded
- refresh tokens should be stored and validated securely
- production secrets should be managed through secure configuration mechanisms

---

## Example Functional Flow

### Login and Token Renewal
1. user logs in through `Identity.API`
2. system returns an access token and refresh token
3. client uses the access token to call protected APIs
4. when the access token expires, the client sends the refresh token to `Identity.API`
5. `Identity.API` validates the refresh token
6. a new access token is issued

### Book Appointment
1. user logs in using `Identity.API`
2. user receives access token and refresh token
3. client requests doctors/schedules
4. client selects doctor and time slot
5. `Appointment.API` checks slot availability through `Availability.API`
6. appointment is created
7. slot is marked reserved/booked
8. event is published
9. `Notification.Worker` processes the event

---

## Testing

This project can be tested using:

- **Swagger UI**
- **Postman**
- manual end-to-end flow testing

Suggested tests:
- register/login flow
- access token generation
- refresh token generation
- token renewal flow
- token-protected endpoint access
- doctor retrieval
- slot retrieval
- successful appointment booking
- appointment cancellation
- notification/event flow
