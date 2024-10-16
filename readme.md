# RonSijm.Visma.PUM.AWS

## Description

A program that lets you sign into PUM and AWS, and lets you store the results in a profile.

On windows you can then just click the profile and login, on Linux this hasn't been tested yet.

## To Fill in:

- Username
	- Your Visma Account Username
	- Example: A12345_CPA@visma.com
- Password
	- Your Visma Account Password
- Profile
	- The AWS Profile to store it in
	- By default 'default'
	- This is the profile you'll also use to invoke the AWS CLI
		- AWS --profile xyz
- Save
	- True
	- False

- If save = true
	- Otp Secret Key (2fa)
- If save = false
	- OTP Token (2fa)
- Region
	- The AWS Region. 
	- For example: eu-central-1
- Role selection
	- Once you successfully logged in, you will see a selection of available roles.
	- Make sure to pick the account that matches your 'profile'
	- Because this account selection will be saved among side of the rest of the data
- Password for file
	- Because the created file contains sensitive data, it is encrypted.
	- So for safety you need to provide a password, that you need to provide again when you load the file.
	- Make sure that your password complies with the Visma Password Policy


## How to find your OTP Secret Key

- Assuming you've already created a OTP, otherwise follow the guide on Confluence.
- If you've scanned the account with Lastpass Authenticator
	- Press the 3 dots on the right of the token, above the count down
	- Press Edit Account
	- There's a secret key input box
	- Press the eye icon to reveal your secret key
	- That's the key you need to generate OTP Token

## Saving

Upon opening the program, the file extension `.AWS-Pum-Profile` will be associated with the program.

When you save a configuration, files will be saved with that extension.

## Loading

You can double click a file, and it should automatically open and ask for a password that you've used to save the profile.

Alternatively you can start the program with command line parameters `-l filename`