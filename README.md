# Authentication API Documentation

Welcome to the official documentation for the Authentication API! This guide provides a comprehensive overview of the available endpoints and functionalities.

## Table of Contents

- [Users](#users)
- [User Name](#user-name)
- [User Registration](#user-registration)
- [Login with Email](#login-with-email)
- [Login with Username](#login-with-username)
- [Login with Phone Number](#login-with-phone-number)
- [Change Password](#change-password)
- [Change Username or Number](#change-username-or-number)
- [User Image](#user-image)
- [Verify User](#verify-user)
- [Forgot Password](#forgot-password)
- [Reset Password](#reset-password)

## Users

- Endpoint: `{{baseUrl}}/api/User/Users`
- Description: Retrieve a list of users.
- Method: `GET`

## User Name

- Endpoint: `{{baseUrl}}/api/User/UserName?username=<string>`
- Description: Retrieve the username for a specific user.
- Method: `GET`

## User Registration

- Endpoint: `{{baseUrl}}/api/User/register`
- Description: Register a new user.
- Method: `POST`

## Login with Email

- Endpoint: `{{baseUrl}}/api/User/loginWithEmail`
- Description: User login using email.
- Method: `POST`

## Login with Username

- Endpoint: `{{baseUrl}}/api/User/loginWithUserName`
- Description: User login using username.
- Method: `POST`

## Login with Phone Number

- Endpoint: `{{baseUrl}}/api/User/loginWithPhoneNumber`
- Description: User login using phone number.
- Method: `POST`

## Change Password

- Endpoint: `{{baseUrl}}/api/User/Change-password?newpassword=<string>&oldpassword=<string>`
- Description: Change the user's password.
- Method: `PUT`

## Change Username or Number

- Endpoint: `{{baseUrl}}/api/User/Change-usernameornumber`
- Description: Change the user's username or phone number.
- Method: `PUT`

## User Image

- Endpoint: `{{baseUrl}}/api/User/userimage`
- Description: Retrieve the user's image.
- Method: `GET`

## Verify User

- Endpoint: `{{baseUrl}}/api/User/verify?token=<string>`
- Description: Verify a user using a token.
- Method: `GET`

## Forgot Password

- Endpoint: `{{baseUrl}}/api/User/Forgot-password?email=<string>`
- Description: Initiate the forgot password process.
- Method: `POST`

## Reset Password

- Endpoint: `{{baseUrl}}/api/User/Reset-password`
- Description: Reset the user's password.
- Method: `POST`

---

