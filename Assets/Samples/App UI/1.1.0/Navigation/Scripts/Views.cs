using System.Linq;
using Unity.AppUI.Core;
using UnityEngine;
using Unity.AppUI.Navigation;
using Unity.AppUI.UI;
using UnityEngine.UIElements;
using Unity.AppUI.Navigation.Generated;
using UnityEngine.Scripting;

namespace Unity.AppUI.Samples.Navigation
{
    [Preserve]
    class SplashScreen : NavigationScreen
    {
        public SplashScreen()
        {
            var content = new Preloader();
            content.StretchToParentSize();

            hierarchy.Add(content);

            schedule.Execute((timer) =>
            {
                var loggedIn = PlayerPrefs.GetInt("loggedIn", 0) > 0;
                if (!loggedIn)
                    this.FindNavController().Navigate(Actions.splash_to_auth);
                else
                    this.FindNavController().Navigate(Actions.splash_to_home);
            }).ExecuteLater(3000);
        }
    }
    
    [Preserve]
    class HomeScreen : NavigationScreen
    {
        public HomeScreen()
        {
            var text = new Unity.AppUI.UI.Text("Home Screen Content");
            text.AddToClassList("text-content");
            Add(text);
            var btn = new Unity.AppUI.UI.Button { title = "Go to Profile Page" };
            btn.clicked += () => 
            {
                this.FindNavController().Navigate(Actions.go_to_profile, new Argument("toto", "tata"));
            };
            Add(btn);

        }
    }
    
    [Preserve]
    class ItemsScreen : NavigationScreen
    {
        public ItemsScreen()
        {
            for (var i = 0; i < 100; i++)
            {
                var item = new Unity.AppUI.UI.Text { text = "Item " + i };
                item.AddToClassList("item-content");
                item.focusable = true;
                var itemName = $"Item {i}";
                var clickable = new Pressable(() =>
                {
                    this.FindNavController()
                        .Navigate(
                            Actions.items_to_item_details, 
                            new Argument("item", itemName));
                });
                item.AddManipulator(clickable);
                Add(item);
            }
        }
    }
    
    [Preserve]
    class ItemDetailScreen : NavigationScreen
    {
        public ItemDetailScreen()
        {
            Add(new Unity.AppUI.UI.Text("Item Detail Screen Content"));
        }

        protected override void OnEnter(NavController controller, NavDestination destination, Argument[] args)
        {
            var item = args.FirstOrDefault(arg => arg.name == "item");
            if (item != null)
            {
                var text = new Unity.AppUI.UI.Text("Item " + item.value);
                text.AddToClassList("text-content");
                Add(text);
            }
        }
    }
    
    [Preserve]
    class LoginScreen : NavigationScreen
    {
        public LoginScreen()
        {
            Add(new Unity.AppUI.UI.Heading("Login"));
            var usernameField = new Unity.AppUI.UI.TextField { placeholder = "Username" };
            Add(usernameField);
            var passwordField = new Unity.AppUI.UI.TextField { placeholder = "Password" };
            Add(passwordField);
            var loginButton = new Unity.AppUI.UI.Button { title = "Login" };
            loginButton.clicked += () => 
            {
                PlayerPrefs.SetInt("loggedIn", 1);
                this.FindNavController().Navigate(Actions.go_to_home);
            };
            Add(loginButton);
            var signUpMessage = new Unity.AppUI.UI.Link("Don't have an account? Sign up");
            signUpMessage.AddToClassList("signup-message");
            Add(signUpMessage);
            signUpMessage.clickable.clicked += () => 
            {
                this.FindNavController().Navigate(Actions.login_to_signup);
            };
            var forgotPasswordMessage = new Unity.AppUI.UI.Link("Forgot password?");
            forgotPasswordMessage.AddToClassList("forgot-password-message");
            Add(forgotPasswordMessage);
            forgotPasswordMessage.clickable.clicked += () => 
            {
                this.FindNavController().Navigate(Actions.login_to_forgot_password);
            };
        }
    }
    
    [Preserve]
    class SignupScreen : NavigationScreen
    {
        public SignupScreen()
        {
            Add(new Unity.AppUI.UI.Heading("Signup"));
            var usernameField = new Unity.AppUI.UI.TextField { placeholder = "Username" };
            Add(usernameField);
            var passwordField = new Unity.AppUI.UI.TextField { placeholder = "Password" };
            Add(passwordField);
            var signupButton = new Unity.AppUI.UI.Button { title = "Signup" };
            signupButton.clicked += () => 
            {
                PlayerPrefs.SetInt("loggedIn", 1);
                this.FindNavController().Navigate(Actions.go_to_home);
            };
            Add(signupButton);
            var loginMessage = new Unity.AppUI.UI.Link("Already have an account? Login");
            loginMessage.AddToClassList("login-message");
            Add(loginMessage);
            loginMessage.clickable.clicked += () => 
            {
                this.FindNavController().PopBackStack();
            };
        }
    }
    
    [Preserve]
    class ForgotPasswordScreen : NavigationScreen
    {
        public ForgotPasswordScreen()
        {
            Add(new Unity.AppUI.UI.Heading("Forgot Password"));
            var usernameField = new Unity.AppUI.UI.TextField { placeholder = "Username" };
            Add(usernameField);
            var sendButton = new Unity.AppUI.UI.Button { title = "Send" };
            sendButton.clicked += () => 
            {
                this.FindNavController().Navigate(Actions.fp_to_confirm_code);
            };
            Add(sendButton);
            var backToLoginMessage = new Unity.AppUI.UI.Link("Back to Login");
            backToLoginMessage.AddToClassList("back-to-login-message");
            Add(backToLoginMessage);
            backToLoginMessage.clickable.clicked += () => 
            {
                this.FindNavController().PopBackStack();
            };
        }
    }

    [Preserve]
    class CodeConfirmScreen : NavigationScreen
    {
        public CodeConfirmScreen()
        {
            Add(new Heading("Code sent!"));
            Add(new Text("Please check your email for the confirmation code."));
            
            var backToLoginButton = new UI.Button { title = "Back to Login" };
            backToLoginButton.clicked += () =>
            {
                this.FindNavController().Navigate(Actions.go_to_login);
            };
            Add(backToLoginButton);
        }
    }
    
    [Preserve]
    class ProfileScreen : NavigationScreen
    {
        public ProfileScreen()
        {
            var settingsButton = new SettingRow("Settings", "");
            settingsButton.clickable.clicked += () => 
            {
                this.FindNavController().Navigate(Actions.profile_to_settings);
            };
            Add(settingsButton);
            var logoutButton = new Unity.AppUI.UI.Button { title = "Logout" };
            logoutButton.clicked += () => 
            {
                PlayerPrefs.SetInt("loggedIn", 0);
                this.FindNavController().Navigate(Actions.go_to_login);
            };
            Add(logoutButton);
        }
        
        protected override void OnEnter(NavController controller, NavDestination destination, Argument[] args)
        {
            if (args is {Length: > 0})
                Debug.Log("Entered Profile Screen with arguments: " + 
                args.Select(a => a.name + " = " + a.value).Aggregate((a, b) => a + ", " + b));
        }
        
        protected override void OnExit(NavController controller, NavDestination destination, Argument[] args)
        {
            Debug.Log("Exited Profile Screen");
        }
    }

    class SettingRow : VisualElement
    {
        Text m_Value;
        
        public Pressable clickable { get; }

        public string value
        {
            get => m_Value.text;
            set => m_Value.text = value;
        }
        
        public SettingRow(string title, string value)
        {
            clickable = new Pressable();
            this.AddManipulator(clickable);
            
            AddToClassList("setting-row");
            var titleLabel = new Unity.AppUI.UI.Text(title) { pickingMode = PickingMode.Ignore };
            titleLabel.AddToClassList("setting-row-title");
            Add(titleLabel);
            m_Value = new Unity.AppUI.UI.Text(value) { pickingMode = PickingMode.Ignore };
            m_Value.AddToClassList("setting-row-value");
            Add(m_Value);
            var chevronIcon = new Icon {iconName = "caret-right", size = IconSize.S, pickingMode = PickingMode.Ignore};
            chevronIcon.AddToClassList("setting-row-chevron");
            Add(chevronIcon);
        }
    }
    
    [Preserve]
    class SettingsScreen : NavigationScreen
    {
        SettingRow m_ThemeSettingRow;
        
        public SettingsScreen()
        {
            m_ThemeSettingRow = new SettingRow("Theme", "");
            m_ThemeSettingRow.AddToClassList("theme-setting-row");
            Add(m_ThemeSettingRow);
        }
        
        protected override void OnEnter(NavController controller, NavDestination destination, Argument[] args)
        {
            var theme = this.GetContext<ThemeContext>().theme;
            m_ThemeSettingRow.value = theme;
            m_ThemeSettingRow.clickable.clicked += () => 
            {
                this.FindNavController().Navigate(Actions.settings_to_theme, new Argument("currentTheme", theme));
            };
        }
    }
    
    [Preserve]
    class ThemeSettingScreen : NavigationScreen
    {
        readonly RadioGroup m_RadioGroup;
        
        public ThemeSettingScreen()
        {
            m_RadioGroup = new RadioGroup();
            var darkThemeButton = new Radio {label = "dark"};
            darkThemeButton.AddToClassList("dark-theme-button");
            var lightThemeButton = new Radio {label = "light"};
            lightThemeButton.AddToClassList("light-theme-button");
            m_RadioGroup.Add(darkThemeButton);
            m_RadioGroup.Add(lightThemeButton);
            Add(m_RadioGroup);
            m_RadioGroup.RegisterValueChangedCallback(OnValueChanged);
        }

        void OnValueChanged(ChangeEvent<int> evt)
        {
            var theme = evt.newValue == 0 ? "dark" : "light";
            var closestProvider = this.GetContextProvider<ThemeContext>();
            closestProvider.ProvideContext(new ThemeContext(theme));
        }

        protected override void OnEnter(NavController controller, NavDestination destination, Argument[] args)
        {
            if (args is {Length: > 0})
            {
                var currentTheme = args[0].value;
                m_RadioGroup.SetValueWithoutNotify(currentTheme == "dark" ? 0 : 1);
            }
        }
    }

    class MyVisualController : INavVisualController
    {
        public void SetupBottomNavBar(BottomNavBar bottomNavBar, NavDestination destination, NavController navController)
        {
            if (!destination.showBottomNavBar)
                return;
            
            var homeButton = new BottomNavBarItem("info", "Home", () => navController.Navigate(Actions.go_to_home))
            {
                isSelected = destination.name == Destinations.home
            };
            bottomNavBar.Add(homeButton);
            
            var itemsButton = new BottomNavBarItem("list", "Items", () => navController.Navigate(Actions.go_to_items))
            {
                isSelected = destination.name == Destinations.items
            };
            bottomNavBar.Add(itemsButton);
            
            var profileButton = new BottomNavBarItem("users", "Profile", () => navController.Navigate(Actions.go_to_profile))
            {
                isSelected = destination.name == Destinations.profile
            };
            bottomNavBar.Add(profileButton);
        }

        public void SetupAppBar(AppBar appBar, NavDestination destination, NavController navController)
        {
            if (!destination.showAppBar)
                return;
            
            appBar.title = destination.label;
            appBar.stretch = true;
            appBar.expandedHeight = 92;
        }

        public void SetupDrawer(Drawer drawer, NavDestination destination, NavController navController)
        {
            if (!destination.showDrawer)
                return;

            drawer.swipeAreaWidth = 16;
            drawer.Add(new DrawerHeader());
            drawer.Add(new Divider { direction = Direction.Horizontal });

            var homeButton = new MenuItem
            {
                icon = "info", 
                label = "Home",
                active = destination.name == Destinations.home
            };
            homeButton.clickable.clicked += () =>
            {
                navController.Navigate(Actions.go_to_home);
                drawer.Close();
            };
            drawer.Add(homeButton);
            
            var itemsButton = new MenuItem
            {
                icon = "list", 
                label = "Items",
                active = destination.name == Destinations.items
            };
            itemsButton.clickable.clicked += () =>
            {
                navController.Navigate(Actions.go_to_items);
                drawer.Close();
            };
            drawer.Add(itemsButton);
            
            var profileButton = new MenuItem
            {
                icon = "users", 
                label = "Profile",
                active = destination.name == Destinations.profile
            };
            profileButton.clickable.clicked += () =>
            {
                navController.Navigate(Actions.go_to_profile);
                drawer.Close();
            };
            drawer.Add(profileButton);
        }
    }
}
