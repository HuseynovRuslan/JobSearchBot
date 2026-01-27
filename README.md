# JobSearch Telegram Bot

A Telegram bot that monitors [JobSearch.az](https://jobsearch.az) for new programming job listings and sends notifications.

## Features

- 🔍 Monitors JobSearch.az every 15 minutes
- 💻 Filters for programming-related jobs (developer, software, python, java, etc.)
- 📱 Sends instant Telegram notifications for new listings
- 🔒 Secure - uses environment variables for credentials

## Setup

### 1. Create a Telegram Bot

1. Open Telegram and search for [@BotFather](https://t.me/BotFather)
2. Send `/newbot` and follow the instructions
3. Save the bot token

### 2. Get Your Chat ID

1. Start a chat with your new bot
2. Send any message to the bot
3. Visit: `https://api.telegram.org/bot<YOUR_TOKEN>/getUpdates`
4. Find your `chat.id` in the response

### 3. Set Environment Variables

**Windows (PowerShell):**
```powershell
$env:TELEGRAM_BOT_TOKEN="your_bot_token_here"
$env:TELEGRAM_CHAT_ID="your_chat_id_here"
```

**Linux/macOS:**
```bash
export TELEGRAM_BOT_TOKEN="your_bot_token_here"
export TELEGRAM_CHAT_ID="your_chat_id_here"
```

### 4. Run the Bot

```bash
dotnet run
```

## Deploy to Cloud (24/7)

For continuous operation, deploy to [Railway.app](https://railway.app):

1. Push code to GitHub
2. Connect Railway to your GitHub repo
3. Add environment variables in Railway dashboard
4. Deploy!

## Keywords Monitored

The bot looks for jobs containing:
- developer, software, programmer, engineer
- backend, frontend, full-stack
- .net, c#, java, python, javascript, react, angular, vue
- devops, cloud, aws, azure, docker
- And more...

## License

MIT
