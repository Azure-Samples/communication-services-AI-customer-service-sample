// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

export async function getEmailTemplate(summaryDetails: string | undefined) {
  const emailBody = `<!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>Power Company</title>
    <style>
        body {
            margin: 0;
            padding: 0;
            font-family: Arial, sans-serif;
            background-color: #f4f4f4;
        }

        .container {
            max-width: 600px;
            margin: 0 auto;
            padding: 20px;
            background-color: #ffffff;
        }

        .header {
            background-color: #00894F;
            color: white;
            text-align: left;
            padding: 1px;
        }

        .header-text {
            margin-left: 10px;
            font-size: 20px;
        }

        .content {
            padding: 20px 20px;
        }

        .footer {
            text-align: left;
            margin-left: 7px;
            padding: 10px 10px;
            color: #888888;
            font-size: 12px;
        }

            .footer a {
                color: #888888;
            }

        .links-content {
            background-color: #ecfcf4;
            padding: 10px;
        }

            .links-content a {
                padding: 20px 10px;
                color: #00894F;
            }

            .links-content p {
                margin-left: 10px;
            }
    </style>
</head>
<body>
    <div class="container">
        <div class="header">
            <p class="header-text"><strong>Power</strong> Company</p>
        </div>
        <div class="content">
            <p>${summaryDetails ? summaryDetails.replace(/\n/g, '<br>') : ''}</p>
        </div>
        <div class="links-content">
            <p>Contact customer Support</p>
            <a href="#">Email</a>
            <a href="#">Call</a>
            <a href="#">SMS</a>
            <a href="#">Chat</a>
        </div>
        <div class="footer">
            <p>Power Company&copy; 2023.</p>
            <p> <a href="#">Privacy statement</a> &nbsp;|&nbsp; <a href="#">Unsubscribe</a></p>
        </div>
    </div>
</body>
</html>
`;

  return emailBody;
}
