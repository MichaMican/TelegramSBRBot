# TelegramSBRBot

## Description
A Telegram messenger bot, with different functions.

## SetUp
- create Telegram bot with [Botfather](https://t.me/botfather)
- create file appsettings.json and put in necessarry details (see [appsettingsTEMPLATE.json](https://github.com/MichaMican/TelegramSBRBot/blob/master/appsettingsTEMPLATE.json))
- setup DB (Run [SQLScripts](https://github.com/MichaMican/TelegramSBRBot/tree/master/SQLScripts) to create all necessary tables)
- deploy the program to a webservice (e.g. Azure WebService)
- run this command in cmd (replace <> with your stuff): 
  ```shell
  curl -F "url=https://<YOURDOMAIN.EXAMPLE>/api/Telegram/new-message" https://api.telegram.org/bot<YOURTOKEN>/setWebhook
  ```


## Special Thanks
Special Thanks to [Joseph Paul](https://jsph.pl/) for letting me use his [RandomUslessFactsAPI](https://uselessfacts.jsph.pl/) as a source for the FunFact "subscription" service

