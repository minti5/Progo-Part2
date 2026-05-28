using Microsoft.Win32;
using System.IO;
using System.Media;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Progo_Part2
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private ChatbotEngine _chatbot = null!;
        private SoundPlayer? _welcomeSound;
        private ChatHistory _chatHistory = null!;
        private string _profileImagePath = string.Empty;

        public MainWindow()
        {
            InitializeComponent();
            _chatbot = new ChatbotEngine();
            _chatHistory = new ChatHistory();

            // Load saved profile picture if exists
            LoadProfilePicture();

            // Play voice greeting when application starts
            PlayVoiceGreeting();

            // Display welcome message
            AddBotMessage("Hello! 👋 I'm your Cybersecurity Awareness Assistant. I'm here to help you stay safe online!\n\n" +
                         "You can ask me about:\n" +
                         "• 🔐 Password Security\n" +
                         "• 🎣 Phishing Scams\n" +
                         "• 👁️ Privacy Protection\n" +
                         "• 🦠 Malware & Viruses\n" +
                         "• 🌐 Secure Browsing\n" +
                         "• 💾 Data Backup\n" +
                         "• 🔑 Two-Factor Authentication (2FA)\n" +
                         "• 🌍 VPN Security\n\n" +
                         "Feel free to share your name or concerns with me!\n\n" +
                         "💡 TIP: All conversations are automatically saved to the 'History' folder as text files!");
        }

        private void LoadProfilePicture()
        {
            try
            {
                string appDataPath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "CybersecurityChatbot");
                string savedImagePath = System.IO.Path.Combine(appDataPath, "profile_image.png");

                if (File.Exists(savedImagePath))
                {
                    var bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri(savedImagePath);
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.EndInit();

                    ProfileImage.Source = bitmap;
                    ProfileImage.Visibility = Visibility.Visible;
                    ProfileIcon.Visibility = Visibility.Collapsed;
                    _profileImagePath = savedImagePath;
                }
                else
                {
                    ProfileImage.Visibility = Visibility.Collapsed;
                    ProfileIcon.Visibility = Visibility.Visible;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading profile picture: {ex.Message}");
                ProfileImage.Visibility = Visibility.Collapsed;
                ProfileIcon.Visibility = Visibility.Visible;
            }
        }

        private void SaveProfilePicture(string sourcePath)
        {
            try
            {
                string appDataPath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "CybersecurityChatbot");
                if (!Directory.Exists(appDataPath))
                {
                    Directory.CreateDirectory(appDataPath);
                }

                string destPath = System.IO.Path.Combine(appDataPath, "profile_image.png");
                File.Copy(sourcePath, destPath, true);
                _profileImagePath = destPath;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error saving profile picture: {ex.Message}");
            }
        }

        private void ProfilePicture_Click(object sender, MouseButtonEventArgs e)
        {
            try
            {
                OpenFileDialog openFileDialog = new OpenFileDialog
                {
                    Title = "Select Profile Picture",
                    Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp;*.gif",
                    FilterIndex = 1
                };

                if (openFileDialog.ShowDialog() == true)
                {
                    var bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri(openFileDialog.FileName);
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.EndInit();

                    ProfileImage.Source = bitmap;
                    ProfileImage.Visibility = Visibility.Visible;
                    ProfileIcon.Visibility = Visibility.Collapsed;

                    SaveProfilePicture(openFileDialog.FileName);

                    AddBotMessage($"📸 Nice! Your profile picture has been updated. It will be saved for future sessions.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading profile picture: {ex.Message}",
                              "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void PlayVoiceGreeting()
        {
            try
            {
                string audioPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "C:\\Users\\Mintirho\\source\\repos\\Progo-Part2\\Progo-Part2\\welcome.wav");

                if (File.Exists(audioPath))
                {
                    _welcomeSound = new SoundPlayer(audioPath);
                    _welcomeSound.Play();
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("Welcome audio file not found.");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error playing voice greeting: {ex.Message}");
            }
        }

        private string GetCurrentTimestamp()
        {
            return DateTime.Now.ToString("HH:mm:ss");
        }

        private void SendButton_Click(object sender, RoutedEventArgs e)
        {
            ProcessUserInput();
        }

        private void InputTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && !Keyboard.IsKeyDown(Key.LeftShift))
            {
                e.Handled = true;
                ProcessUserInput();
            }
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            ChatStackPanel.Children.Clear();
            AddBotMessage("Chat cleared! How can I help you with cybersecurity today? 🔒");
        }

        private void ViewHistoryButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string filePath = _chatHistory.GetCurrentSessionFilePath();
                if (File.Exists(filePath))
                {
                    System.Diagnostics.Process.Start("notepad.exe", filePath);
                }
                else
                {
                    MessageBox.Show("No history file found yet. Start a conversation first!",
                                  "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening history: {ex.Message}",
                              "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ProcessUserInput()
        {
            string userInput = InputTextBox.Text.Trim();

            if (string.IsNullOrWhiteSpace(userInput))
            {
                AddBotMessage("Please enter a message. I'm here to help with cybersecurity topics! 😊");
                return;
            }

            // Add user message to chat with timestamp
            AddUserMessage(userInput);

            // Clear input box
            InputTextBox.Clear();

            // Get bot response
            string response = _chatbot.GetResponse(userInput);

            // Add bot response with timestamp
            AddBotMessage(response);

            // Auto-scroll to bottom
            ScrollToBottom();
        }

        private void AddUserMessage(string message)
        {
            // Save to history
            _chatHistory.AddMessage("You", message);

            // Main container for user message
            DockPanel messageContainer = new DockPanel
            {
                Margin = new Thickness(0, 5, 0, 5),
                HorizontalAlignment = HorizontalAlignment.Right,
                LastChildFill = false
            };

            // User avatar
            Border avatarBorder = new Border
            {
                Width = 35,
                Height = 35,
                CornerRadius = new CornerRadius(17.5),
                Background = (Brush)new BrushConverter().ConvertFrom("#3498DB"),
                Margin = new Thickness(5, 0, 5, 0),
                VerticalAlignment = VerticalAlignment.Bottom,
                HorizontalAlignment = HorizontalAlignment.Right
            };

            // Check if user has profile picture
            if (!string.IsNullOrEmpty(_profileImagePath) && File.Exists(_profileImagePath))
            {
                try
                {
                    var avatarImage = new Image
                    {
                        Width = 35,
                        Height = 35,
                        Stretch = Stretch.UniformToFill
                    };

                    var bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri(_profileImagePath);
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.EndInit();
                    avatarImage.Source = bitmap;

                    avatarBorder.Child = avatarImage;
                }
                catch
                {
                    TextBlock avatarText = new TextBlock
                    {
                        Text = "👤",
                        FontSize = 18,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center,
                        Foreground = Brushes.White
                    };
                    avatarBorder.Child = avatarText;
                }
            }
            else
            {
                TextBlock avatarText = new TextBlock
                {
                    Text = "👤",
                    FontSize = 18,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    Foreground = Brushes.White
                };
                avatarBorder.Child = avatarText;
            }

            // Message content panel
            StackPanel messageContent = new StackPanel
            {
                HorizontalAlignment = HorizontalAlignment.Right
            };

            // Timestamp for user message
            TextBlock timestamp = new TextBlock
            {
                Text = $"You • {GetCurrentTimestamp()}",
                FontSize = 9,
                Foreground = (SolidColorBrush)new BrushConverter().ConvertFrom("#7F8C8D"),
                HorizontalAlignment = HorizontalAlignment.Right,
                Margin = new Thickness(0, 0, 10, 2)
            };

            // Message bubble
            Border messageBorder = new Border
            {
                Style = (Style)FindResource("ChatBubbleUser"),
                HorizontalAlignment = HorizontalAlignment.Right
            };

            TextBlock textBlock = new TextBlock
            {
                Text = message,
                Foreground = Brushes.White,
                FontSize = 13,
                TextWrapping = TextWrapping.Wrap,
                MaxWidth = 400
            };

            messageBorder.Child = textBlock;
            messageContent.Children.Add(timestamp);
            messageContent.Children.Add(messageBorder);

            DockPanel.SetDock(avatarBorder, Dock.Right);
            DockPanel.SetDock(messageContent, Dock.Right);

            messageContainer.Children.Add(avatarBorder);
            messageContainer.Children.Add(messageContent);

            ChatStackPanel.Children.Add(messageContainer);
        }

        private void AddBotMessage(string message)
        {
            // Save to history
            _chatHistory.AddMessage("CyberGuard AI", message);

            // Main container for bot message
            DockPanel messageContainer = new DockPanel
            {
                Margin = new Thickness(0, 5, 0, 5),
                HorizontalAlignment = HorizontalAlignment.Left,
                LastChildFill = false
            };

            // Bot avatar
            Border avatarBorder = new Border
            {
                Width = 35,
                Height = 35,
                CornerRadius = new CornerRadius(17.5),
                Background = (Brush)new BrushConverter().ConvertFrom("#9B59B6"),
                Margin = new Thickness(5, 0, 5, 0),
                VerticalAlignment = VerticalAlignment.Bottom,
                HorizontalAlignment = HorizontalAlignment.Left
            };

            TextBlock avatarText = new TextBlock
            {
                Text = "🤖",
                FontSize = 18,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Foreground = Brushes.White
            };
            avatarBorder.Child = avatarText;

            // Message content panel
            StackPanel messageContent = new StackPanel
            {
                HorizontalAlignment = HorizontalAlignment.Left
            };

            // Timestamp for bot message
            TextBlock timestamp = new TextBlock
            {
                Text = $"CyberGuard AI • {GetCurrentTimestamp()}",
                FontSize = 9,
                Foreground = (SolidColorBrush)new BrushConverter().ConvertFrom("#7F8C8D"),
                HorizontalAlignment = HorizontalAlignment.Left,
                Margin = new Thickness(10, 0, 0, 2)
            };

            // Message bubble
            Border messageBorder = new Border
            {
                Style = (Style)FindResource("ChatBubbleBot"),
                HorizontalAlignment = HorizontalAlignment.Left
            };

            StackPanel contentPanel = new StackPanel();

            // Bot icon and name
            TextBlock header = new TextBlock
            {
                Text = "🤖 CyberGuard AI",
                Foreground = (SolidColorBrush)new BrushConverter().ConvertFrom("#2C3E50"),
                FontSize = 11,
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(0, 0, 0, 5)
            };

            TextBlock messageText = new TextBlock
            {
                Text = message,
                Foreground = Brushes.Black,
                FontSize = 13,
                TextWrapping = TextWrapping.Wrap,
                MaxWidth = 400
            };

            contentPanel.Children.Add(header);
            contentPanel.Children.Add(messageText);
            messageBorder.Child = contentPanel;

            messageContent.Children.Add(timestamp);
            messageContent.Children.Add(messageBorder);

            DockPanel.SetDock(avatarBorder, Dock.Left);
            DockPanel.SetDock(messageContent, Dock.Left);

            messageContainer.Children.Add(avatarBorder);
            messageContainer.Children.Add(messageContent);

            ChatStackPanel.Children.Add(messageContainer);
        }

        private void ScrollToBottom()
        {
            ChatScrollViewer.ScrollToBottom();
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            _welcomeSound?.Dispose();
            _chatHistory.SaveOnExit();
        }
    }

    // ChatHistory Class
    public class ChatMessage
    {
        public string Sender { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string Timestamp { get; set; } = string.Empty;
        public string FullTimestamp { get; set; } = string.Empty;
    }

    public class ChatHistory
    {
        private readonly string _historyFolder;
        private List<ChatMessage> _currentSessionMessages;
        private DateTime _sessionStartTime;
        private string _currentSessionFileName;

        public ChatHistory()
        {
            // Create History folder in the application directory
            _historyFolder = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "History");
            if (!Directory.Exists(_historyFolder))
            {
                Directory.CreateDirectory(_historyFolder);
            }

            _currentSessionMessages = new List<ChatMessage>();
            _sessionStartTime = DateTime.Now;
            _currentSessionFileName = GenerateFileName();

            // Create initial session file
            CreateSessionFile();
        }

        private string GenerateFileName()
        {
            return $"ChatHistory_{_sessionStartTime:yyyyMMdd_HHmmss}.txt";
        }

        private void CreateSessionFile()
        {
            try
            {
                string filePath = System.IO.Path.Combine(_historyFolder, _currentSessionFileName);
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("=".PadRight(100, '='));
                sb.AppendLine($"CYBERSECURITY CHATBOT CONVERSATION");
                sb.AppendLine($"Session started: {_sessionStartTime:yyyy-MM-dd HH:mm:ss}");
                sb.AppendLine($"Session ID: {Guid.NewGuid()}");
                sb.AppendLine("=".PadRight(100, '='));
                sb.AppendLine();
                sb.AppendLine("Chatbot: Hello! 👋 I'm your Cybersecurity Awareness Assistant.");
                sb.AppendLine();

                File.WriteAllText(filePath, sb.ToString(), Encoding.UTF8);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error creating session file: {ex.Message}");
            }
        }

        public void AddMessage(string sender, string message)
        {
            var chatMessage = new ChatMessage
            {
                Sender = sender,
                Message = message,
                Timestamp = DateTime.Now.ToString("HH:mm:ss"),
                FullTimestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
            };

            _currentSessionMessages.Add(chatMessage);

            // Append message to text file
            AppendMessageToFile(chatMessage);
        }

        private void AppendMessageToFile(ChatMessage message)
        {
            try
            {
                string filePath = System.IO.Path.Combine(_historyFolder, _currentSessionFileName);
                StringBuilder sb = new StringBuilder();
                sb.AppendLine($"[{message.Timestamp}] {message.Sender}:");
                sb.AppendLine($"  {message.Message}");
                sb.AppendLine();

                File.AppendAllText(filePath, sb.ToString(), Encoding.UTF8);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error appending message to file: {ex.Message}");
            }
        }

        public void SaveOnExit()
        {
            try
            {
                string filePath = System.IO.Path.Combine(_historyFolder, _currentSessionFileName);
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("=".PadRight(100, '='));
                sb.AppendLine($"Session ended: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
                sb.AppendLine($"Total messages: {_currentSessionMessages.Count}");
                sb.AppendLine($"Session duration: {(DateTime.Now - _sessionStartTime).TotalMinutes:F1} minutes");
                sb.AppendLine("=".PadRight(100, '='));

                File.AppendAllText(filePath, sb.ToString(), Encoding.UTF8);

                System.Diagnostics.Debug.WriteLine($"Chat history saved to: {filePath}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error saving session: {ex.Message}");
            }
        }

        public string GetCurrentSessionFilePath()
        {
            return System.IO.Path.Combine(_historyFolder, _currentSessionFileName);
        }

        public List<string> GetAllHistoryFiles()
        {
            try
            {
                var files = Directory.GetFiles(_historyFolder, "*.txt");
                return new List<string>(files);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error getting history files: {ex.Message}");
                return new List<string>();
            }
        }

        public void OpenHistoryFolder()
        {
            try
            {
                if (Directory.Exists(_historyFolder))
                {
                    System.Diagnostics.Process.Start("explorer.exe", _historyFolder);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error opening history folder: {ex.Message}");
            }
        }

        public void OpenCurrentSessionFile()
        {
            try
            {
                string filePath = GetCurrentSessionFilePath();
                if (File.Exists(filePath))
                {
                    System.Diagnostics.Process.Start("notepad.exe", filePath);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error opening session file: {ex.Message}");
            }
        }
    }

    // Enhanced Chatbot Engine Class with Memory and Conversation Flow
    public class ChatbotEngine
    {
        private Dictionary<string, string> _userMemory = new Dictionary<string, string>();
        private Dictionary<string, object> _userPreferences = new Dictionary<string, object>();
        private string _currentTopic = "";
        private string _lastResponse = "";
        private List<string> _conversationHistory = new List<string>();
        private Dictionary<string, List<string>> _keywordResponses = new Dictionary<string, List<string>>();
        private Dictionary<string, List<string>> _randomResponses = new Dictionary<string, List<string>>();
        private Dictionary<string, List<string>> _sentimentPatterns = new Dictionary<string, List<string>>();

        public ChatbotEngine()
        {
            InitializeResponses();
            InitializeSentimentPatterns();
        }

        private void InitializeResponses()
        {
            _keywordResponses = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase)
            {
                ["password"] = new List<string>
                {
                    "🔐 Password Security Tip: Use strong, unique passwords (12+ characters) with a mix of uppercase, lowercase, numbers, and symbols. Consider using a password manager!",
                    "Never reuse passwords across different accounts. Each account should have its own unique password.",
                    "Enable Two-Factor Authentication (2FA) whenever possible - it adds an extra layer of security beyond just your password."
                },

                ["phish"] = new List<string>
                {
                    "🎣 Phishing Alert: Never click on suspicious links in emails or messages. Always verify the sender's email address carefully.",
                    "Legitimate companies never ask for your password or personal information via email. When in doubt, contact them directly through their official website.",
                    "Check for red flags: urgent language, spelling errors, mismatched URLs, and requests for sensitive information."
                },

                ["scam"] = new List<string>
                {
                    "⚠️ Scam Prevention: Be skeptical of unsolicited calls, emails, or messages promising prizes or threatening account closure.",
                    "Never share verification codes, OTPs, or financial information with anyone who contacts you unexpectedly.",
                    "Research before you invest! If something sounds too good to be true, it probably is."
                },

                ["privacy"] = new List<string>
                {
                    "👁️ Privacy Protection: Review privacy settings on social media regularly. Limit what you share publicly.",
                    "Use a VPN on public Wi-Fi to encrypt your internet traffic and protect your browsing habits.",
                    "Be mindful of what you post online - once something is on the internet, it's difficult to remove completely."
                },

                ["malware"] = new List<string>
                {
                    "🦠 Malware Protection: Keep your operating system and software updated. Enable automatic updates when possible.",
                    "Only download software from official sources. Avoid cracked software or suspicious download sites.",
                    "Use reputable antivirus software and run regular scans on your devices."
                },

                ["secure"] = new List<string>
                {
                    "🌐 Secure Browsing: Look for 'https://' and the padlock icon in your browser's address bar before entering sensitive information.",
                    "Clear your browser cache and cookies regularly, especially when using shared computers.",
                    "Use private browsing mode for sensitive searches, but remember it doesn't make you completely anonymous online."
                },

                ["backup"] = new List<string>
                {
                    "💾 Data Backup: Follow the 3-2-1 backup rule: 3 copies of your data, on 2 different media types, with 1 copy off-site.",
                    "Test your backups regularly to ensure you can restore your data when needed.",
                    "Consider cloud backup services with encryption for important files."
                },

                ["2fa"] = new List<string>
                {
                    "🔐 Two-Factor Authentication adds an extra layer of security. Even if someone steals your password, they can't access your account without the second factor!",
                    "Enable 2FA on all important accounts - email, banking, social media, and cloud storage.",
                    "Use authenticator apps like Google Authenticator or Microsoft Authenticator instead of SMS for better security."
                },

                ["vpn"] = new List<string>
                {
                    "🌐 A VPN encrypts your internet traffic and hides your IP address. Essential for public Wi-Fi!",
                    "Choose a reputable VPN provider that doesn't keep logs of your activity.",
                    "Always use VPN when connecting to public Wi-Fi at coffee shops, airports, or hotels."
                }
            };

            _randomResponses = new Dictionary<string, List<string>>
            {
                ["tip"] = new List<string>
                {
                    "💡 Tip: Enable automatic software updates to receive critical security patches promptly.",
                    "💡 Tip: Use different email addresses for different purposes (personal, shopping, social media).",
                    "💡 Tip: Regularly review app permissions on your phone and revoke unnecessary access.",
                    "💡 Tip: Create a 'security question' with false answers that only you would know.",
                    "💡 Tip: Lock your computer screen whenever you step away from your desk (Windows + L).",
                    "💡 Tip: Back up your important files to at least two different locations - one local and one cloud.",
                    "💡 Tip: Be cautious of QR codes in public places - they could lead to malicious websites.",
                    "💡 Tip: Use a dedicated credit card for online shopping with a low credit limit."
                },

                ["protect"] = new List<string>
                {
                    "🛡️ Protect yourself by being cautious about what you download and install.",
                    "🛡️ Use a password manager to generate and store strong, unique passwords.",
                    "🛡️ Enable biometric authentication (fingerprint/face ID) on your mobile devices."
                }
            };
        }

        private void InitializeSentimentPatterns()
        {
            _sentimentPatterns = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase)
            {
                ["worried"] = new List<string> { "worried", "anxious", "nervous", "scared", "afraid", "concerned", "fear" },
                ["frustrated"] = new List<string> { "frustrated", "annoyed", "angry", "mad", "upset", "tired" },
                ["curious"] = new List<string> { "curious", "interested", "eager", "excited", "want to learn", "tell me" },
                ["confused"] = new List<string> { "confused", "don't understand", "unclear", "what does this mean", "explain" }
            };
        }

        private string GetTopicDisplayName(string topic)
        {
            switch (topic.ToLower())
            {
                case "password": return "password security";
                case "phish": return "phishing prevention";
                case "scam": return "scam awareness";
                case "privacy": return "privacy protection";
                case "malware": return "malware protection";
                case "secure": return "secure browsing";
                case "backup": return "data backup";
                case "2fa": return "two-factor authentication";
                case "vpn": return "VPN security";
                default: return topic;
            }
        }

        // Enhanced memory update
        private void UpdateMemory(string userInput)
        {
            string lowerInput = userInput.ToLower();

            // Remember user's name
            Match nameMatch = Regex.Match(userInput, @"(?:my name is|i am|i'm|called|this is|name's)\s+(\w+)", RegexOptions.IgnoreCase);
            if (nameMatch.Success)
            {
                string name = nameMatch.Groups[1].Value;
                if (!_userMemory.ContainsKey("name"))
                {
                    _userMemory["name"] = name;
                    _lastResponse = $"Nice to meet you, {name}! I'll remember your name for our conversation. 😊";
                }
            }

            // Remember user's interests
            foreach (var topic in _keywordResponses.Keys)
            {
                if (lowerInput.Contains(topic))
                {
                    if (!_userMemory.ContainsKey("interest"))
                    {
                        _userMemory["interest"] = topic;
                        _lastResponse = $"Great! I'll remember that you're interested in {GetTopicDisplayName(topic)}. It's a crucial part of staying safe online! 👍";
                    }
                    _currentTopic = topic;
                    break;
                }
            }

            // Remember experience level
            if (lowerInput.Contains("beginner") || lowerInput.Contains("new") || lowerInput.Contains("just started"))
            {
                _userPreferences["experience"] = "beginner";
            }
            else if (lowerInput.Contains("expert") || lowerInput.Contains("advanced") || lowerInput.Contains("know a lot"))
            {
                _userPreferences["experience"] = "expert";
            }

            // Store conversation history
            _conversationHistory.Add(userInput);
            if (_conversationHistory.Count > 10)
            {
                _conversationHistory.RemoveAt(0);
            }
        }

        private string DetectSentiment(string input)
        {
            input = input.ToLower();

            foreach (var sentiment in _sentimentPatterns)
            {
                foreach (var keyword in sentiment.Value)
                {
                    if (input.Contains(keyword))
                    {
                        return sentiment.Key;
                    }
                }
            }
            return "neutral";
        }

        private string GetSentimentResponse(string sentiment)
        {
            switch (sentiment)
            {
                case "worried":
                    return "I understand your concern. It's completely normal to feel worried about cybersecurity threats. Let me provide some information to help ease your mind:\n\n";
                case "frustrated":
                    return "I hear your frustration. Cybersecurity can be challenging, but let me help simplify things for you:\n\n";
                case "curious":
                    return "That's great that you're curious about cybersecurity! An inquisitive mindset is the first step to staying safe online. Here's what you should know:\n\n";
                case "confused":
                    return "No worries - let me explain this in simpler terms:\n\n";
                default:
                    return "";
            }
        }

        // Enhanced conversation flow with follow-up handling
        private string HandleFollowUp(string userInput)
        {
            string lowerInput = userInput.ToLower();

            // Handle "tell me more", "explain more", etc.
            if (lowerInput.Contains("tell me more") || lowerInput.Contains("explain more") ||
                lowerInput.Contains("more about") || lowerInput.Contains("elaborate") ||
                lowerInput.Contains("continue"))
            {
                if (!string.IsNullOrEmpty(_currentTopic) && _keywordResponses.ContainsKey(_currentTopic))
                {
                    var responses = _keywordResponses[_currentTopic];
                    Random rand = new Random();
                    string moreInfo = responses[rand.Next(responses.Count)];

                    if (_userMemory.ContainsKey("name"))
                    {
                        moreInfo = $"\n\nSince you're interested in learning more, {_userMemory["name"]}, here's additional information:\n\n{moreInfo}";
                    }

                    return GetSentimentResponse(DetectSentiment(userInput)) + moreInfo;
                }
                else
                {
                    return "What specific topic would you like to know more about? (e.g., passwords, phishing, privacy, malware, 2FA, VPN)";
                }
            }

            // Handle "another tip", "another one"
            if (lowerInput.Contains("another tip") || lowerInput.Contains("another one") ||
                lowerInput.Contains("give me another") || lowerInput.Contains("more tips"))
            {
                if (_randomResponses.ContainsKey("tip") && _randomResponses["tip"].Count > 0)
                {
                    Random rand = new Random();
                    string tip = _randomResponses["tip"][rand.Next(_randomResponses["tip"].Count)];

                    if (_userMemory.ContainsKey("name"))
                    {
                        tip = $"Here's another cybersecurity tip for you, {_userMemory["name"]}! {tip}";
                    }
                    else
                    {
                        tip = $"Here's another cybersecurity tip! {tip}";
                    }

                    return tip;
                }
            }

            // Handle confusion
            if (lowerInput.Contains("i don't understand") || lowerInput.Contains("confused"))
            {
                return "I apologize for the confusion. Let me simplify: Cybersecurity is about protecting your digital information. Would you like me to explain any specific topic in simpler terms?";
            }

            // Handle summary request
            if (lowerInput.Contains("summarize") || lowerInput.Contains("recap") || lowerInput.Contains("summary"))
            {
                if (!string.IsNullOrEmpty(_currentTopic))
                {
                    return $"Let me summarize what we discussed about {GetTopicDisplayName(_currentTopic)}: The key points are to stay vigilant, use strong security measures, and always think before clicking on suspicious links. Would you like to explore another topic?";
                }
                else
                {
                    return "We haven't discussed any specific topic yet. What would you like to learn about?";
                }
            }

            return null;
        }

        // Personalize response based on memory
        private string PersonalizeResponse(string response)
        {
            string personalized = response;

            if (_userMemory.ContainsKey("name") && !personalized.Contains(_userMemory["name"]))
            {
                Random rand = new Random();
                string[] prefixes = {
                    $"\n\nBy the way, {_userMemory["name"]}, ",
                    $"\n\n{_userMemory["name"]}, here's something important: ",
                    $"\n\nSpeaking of which, {_userMemory["name"]}, "
                };

                if (rand.Next(3) == 0)
                {
                    personalized = prefixes[rand.Next(prefixes.Length)] + personalized;
                }
            }

            return personalized;
        }

        public string GetResponse(string userInput)
        {
            // Update memory first
            UpdateMemory(userInput);

            // Check if this is a response to memory update
            if (!string.IsNullOrEmpty(_lastResponse))
            {
                string tempResponse = _lastResponse;
                _lastResponse = "";
                return tempResponse;
            }

            // Check for follow-up questions
            string? followUpResponse = HandleFollowUp(userInput);
            if (followUpResponse != null)
            {
                return PersonalizeResponse(followUpResponse);
            }

            // Check for greeting
            if (Regex.IsMatch(userInput, @"\b(hi|hello|hey|greetings|good morning|good afternoon)\b", RegexOptions.IgnoreCase))
            {
                string name = _userMemory.ContainsKey("name") ? _userMemory["name"] : "there";

                if (_userMemory.ContainsKey("interest"))
                {
                    return $"Hello {name}! 👋 Great to see you again! Ready to learn more about {GetTopicDisplayName(_userMemory["interest"])}? I can also help with passwords, phishing, privacy, malware, 2FA, VPN, and more!";
                }
                else
                {
                    return $"Hello {name}! 👋 How can I assist you with cybersecurity today? I can help with passwords, phishing, privacy, malware, 2FA, VPN, secure browsing, and more!";
                }
            }

            // Check for thanks
            if (Regex.IsMatch(userInput, @"\b(thank|thanks|appreciate|helpful)\b", RegexOptions.IgnoreCase))
            {
                if (_userMemory.ContainsKey("name"))
                {
                    return $"You're very welcome, {_userMemory["name"]}! 😊 Stay safe online, and remember I'm here whenever you need cybersecurity advice! Would you like to learn about another topic?";
                }
                else
                {
                    return "You're very welcome! 😊 Stay safe online, and remember I'm here whenever you need cybersecurity advice!";
                }
            }

            // Detect sentiment
            string sentiment = DetectSentiment(userInput);
            string sentimentPrefix = GetSentimentResponse(sentiment);

            // Check for keyword matches
            foreach (var keyword in _keywordResponses.Keys)
            {
                if (userInput.ToLower().Contains(keyword))
                {
                    Random rand = new Random();
                    var responses = _keywordResponses[keyword];
                    string response = responses[rand.Next(responses.Count)];

                    if (_userMemory.ContainsKey("name") || _userMemory.ContainsKey("interest"))
                    {
                        string personalization = "";
                        if (_userMemory.ContainsKey("name") && _userMemory.ContainsKey("interest"))
                        {
                            personalization = $"\n\n[As someone interested in {GetTopicDisplayName(_userMemory["interest"])}, {_userMemory["name"]}, ";
                        }
                        else if (_userMemory.ContainsKey("name"))
                        {
                            personalization = $"\n\n[Thanks for asking, {_userMemory["name"]}! ";
                        }
                        else if (_userMemory.ContainsKey("interest"))
                        {
                            personalization = $"\n\n[Since you're interested in {GetTopicDisplayName(_userMemory["interest"])}, ";
                        }

                        response = sentimentPrefix + response + personalization + "keep prioritizing your online safety!]\n\nWould you like me to explain more or give you another tip?";
                    }
                    else
                    {
                        response = sentimentPrefix + response + "\n\nWould you like me to explain more about this topic?";
                    }

                    return PersonalizeResponse(response);
                }
            }

            // Check for random tip requests
            if (userInput.ToLower().Contains("tip") || userInput.ToLower().Contains("advice") ||
                userInput.ToLower().Contains("suggestion") || userInput.ToLower().Contains("recommend"))
            {
                if (_randomResponses.ContainsKey("tip") && _randomResponses["tip"].Count > 0)
                {
                    Random rand = new Random();
                    string tip = _randomResponses["tip"][rand.Next(_randomResponses["tip"].Count)];

                    if (_userMemory.ContainsKey("name"))
                    {
                        tip = $"Here's a personalized tip for you, {_userMemory["name"]}! {tip}";
                    }

                    return sentimentPrefix + tip + "\n\nWould you like another tip?";
                }
                else
                {
                    return sentimentPrefix + "Here's a quick tip: Always think before you click! Be cautious of unexpected links or attachments, even from people you know.\n\nWould you like more tips?";
                }
            }

            // Default response
            string defaultResponse = sentimentPrefix + "I'm not sure I understand. Could you please rephrase or ask me about specific cybersecurity topics like:\n" +
                   "• 🔐 Password security\n" +
                   "• 🎣 Phishing scams\n" +
                   "• 👁️ Privacy protection\n" +
                   "• 🦠 Malware prevention\n" +
                   "• 🌐 Secure browsing\n" +
                   "• 💾 Data backup\n" +
                   "• 🔑 Two-Factor Authentication (2FA)\n" +
                   "• 🌍 VPN security\n\n" +
                   "Or just ask for a general tip! 😊";

            if (_userMemory.ContainsKey("name"))
            {
                defaultResponse = $"{_userMemory["name"]}, " + defaultResponse.ToLower();
            }

            return defaultResponse;
        }
    }
}