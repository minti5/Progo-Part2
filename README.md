# 🛡️ Cybersecurity Awareness Chatbot

A WPF-based desktop chatbot that educates users about cybersecurity through interactive conversation.

## 📋 Features

- **Chatbot Interface** - Modern chat UI with message bubbles and timestamps
- **Keyword Recognition** - Responds to password, phishing, scam, privacy, malware, 2FA, and VPN topics
- **Random Responses** - Multiple variations for engaging conversations
- **Memory & Recall** - Remembers user name and interests
- **Sentiment Detection** - Detects worried, frustrated, curious, and confused emotions
- **Conversation Flow** - Handles follow-ups like "tell me more" and "another tip"
- **Chat History** - Automatically saves all conversations to text files
- **Profile Picture** - Upload and save custom avatar
- **Voice Greeting** - Plays welcome audio on startup
- **ASCII Art Header** - Cybersecurity-themed header design

## 🚀 How to Run

### Prerequisites
- Windows 10/11
- .NET 6.0 or later

### Steps
1. Clone or download this repository
2. Open `Progo-Part2` in Visual Studio
3. Press `F5` to build and run

## 💬 Example Usage
User: "Hi, my name is John"
Bot: "Nice to meet you, John! I'll remember your name."

User: "Tell me about password security"
Bot: "🔐 Use strong, unique passwords with 12+ characters..."

User: "Tell me more"
Bot: "Never reuse passwords across different accounts..."

User: "Another tip"
Bot: "💡 Enable Two-Factor Authentication whenever possible..."

# 📁 Project Structure
CybersecurityChatbot/
├── MainWindow.xaml # GUI layout
├── MainWindow.xaml.cs # Main logic and chatbot engine
├── CybersecurityChatbot.csproj
├── History/ # Chat history folder (auto-created)
└── README.md


## 🛠️ Technologies

- C# .NET 6.0
- Windows Presentation Foundation (WPF)
- System.Media (audio playback)
- System.IO (file operations)

## 📝 Commands You Can Use

| Command | What It Does |
|---------|---------------|
| `Hi` / `Hello` | Greeting response |
| `My name is...` | Saves your name |
| `I'm interested in...` | Saves your favorite topic |
| `Tell me about passwords` | Password security tips |
| `Tell me more` | More info on current topic |
| `Another tip` | Random cybersecurity tip |
| `I'm worried about...` | Empathetic response |
| `Summarize` | Recap of discussion |

## 📂 Chat History

All conversations are automatically saved to the `History` folder as text files. Each file is named with a timestamp: `ChatHistory_20240528_143025.txt`

Click **"VIEW HISTORY"** in the app to open the current session in Notepad.

## 🖼️ Profile Picture

Click the circular avatar in the top-right corner to upload your own profile picture. It will be saved for future sessions.

## 🔊 Voice Greeting

To enable voice greeting, add a `welcome.wav` file to the application folder with your recorded welcome message.

## ❓ Troubleshooting

**Build Error - "Path ambiguous reference"**
- Use `System.IO.Path.Combine()` instead of `Path.Combine()`

**File locked error**
- Close the application and rebuild
- Or run: `taskkill /F /IM CybersecurityChatbot.exe`

## 📄 License

MIT License - Free for personal and educational use.

## 👤 Author

[Mintirho Lebese]
- GitHub: [minti5](https://github.com/minti5)

---

**Stay safe online! 🔒**
