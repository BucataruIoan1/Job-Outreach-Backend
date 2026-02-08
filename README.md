# Job Outreach Backend

This is the backend for the Job Outreach application, built with ASP.NET Core.

## Configuration

Before running the application, you must configure your email settings.

### 1. Gmail Configuration (App Password)

To send emails using your Gmail account, you need to generate an **App Password**. You cannot use your regular Gmail password.

1.  Go to your [Google Account](https://myaccount.google.com/).
2.  Navigate to **Security**.
3.  Under "How you sign in to Google", select **2-Step Verification** (if not already enabled, you must enable it).
4.  At the bottom of the 2-Step Verification page, select **App passwords**.
5.  Enter a name for the app (e.g., "Job Outreach App") and click **Create**.
6.  Copy the generated 16-character password (it will look like `xxxx xxxx xxxx xxxx`).

### 2. Update Application Settings

1. Create a local appsettings.json file based on appsettings.example.json.
2.  Locate the `Smtp` section:
    ```json
    "Smtp": {
      "Host": "smtp.gmail.com",
      "Port": "587",
      "Username": "add your address email here",
      "Password": "add your app password here"
    }
    ⚠️ Never commit real SMTP credentials or connection strings to source control.
3.  Replace `"add your address email here"` with your full Gmail address (e.g., `yourname@gmail.com`).
4.  Replace `"add your app password here"` with the 16-character App Password you generated in step 1.

**Important:** The application is configured to validate that emails are sent from the configured address. Ensure the `Username` matches the account you intend to send from.

## Database Setup

1.  Ensure you have SQL Server installed and running.
2.  Check the `ConnectionStrings` in `appsettings.json`. The default is:
    ```json
    "ConnectionStrings": {
      "DefaultConnection": "Server=.;Database=JobOutreach;Trusted_Connection=True;TrustServerCertificate=True;"
    }
3.  Run database migrations (if applicable) or ensure the database `JobOutreach` exists.

## Running the Application

1.  Navigate to the `JobOutreach.Api` directory or the root directory.
2.  Run the application using the .NET CLI:
    ```bash
    dotnet run --project JobOutreach.Api
    ```
3.  The API will start (default: `http://localhost:5088`).

## API Overview

The API provides endpoints for managing companies, positions, and sending emails.

### Email Controller (`/api/Email`)
-   **GET /api/Email**: Get all email templates.
-   **GET /api/Email?id={id}**: Get a specific email template.
-   **POST /api/Email**: Create a new email template (requires PDF file).
-   **PUT /api/Email/{id}**: Update an email template (supports optional PDF file update).
-   **DELETE /api/Email/{id}**: Delete an email template.
-   **GET /api/Email/{id}/attachment**: Download the attachment for an email template.
-   **POST /api/Email/send**: Send emails to selected companies using a template.

### Company Controller (`/api/Company`)
-   **GET /api/Company**: Get paginated list of companies.
-   **GET /api/Company/uncontacted**: Get paginated list of uncontacted companies.
-   **POST /api/Company**: Add a new company.
-   **PUT /api/Company/{id}**: Update company details.
-   **DELETE /api/Company/{id}**: Delete a company.

### Position Controller (`/api/Position`)
-   **GET /api/Position**: Get all positions.
-   **POST /api/Position**: Create a new position.
-   **PUT /api/Position/{id}**: Update a position.
-   **DELETE /api/Position/{id}**: Delete a position.

### EmailSent Controller (`/api/EmailSent`)
-   **GET /api/EmailSent**: Get history of sent emails.
-   **DELETE /api/EmailSent/{id}**: Delete a sent email record.
