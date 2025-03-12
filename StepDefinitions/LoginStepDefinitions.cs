//using OpenQA.Selenium;
//using SwagProject.Pages;
//using SwagProject.Hooks;
//using TechTalk.SpecFlow;

//namespace SwagProject.StepDefinitions
//{
//    [Binding]
//    public class LoginStepDefinitions
//    {
//        private readonly IWebDriver driver;
//        private readonly LoginPage loginPage;

//        public LoginStepDefinitions()
//        {
//            driver = Hooks.driver;  // ✅ Correct way to access static field
//            if (driver == null)
//            {
//                throw new Exception("WebDriver is not initialized. Ensure [BeforeScenario] runs first.");
//            }

//            loginPage = new LoginPage(driver);
//        }


//        [Given(@"User is on swag login page")]
//        public void GivenUserIsOnSwagLoginPage()
//        {
//            loginPage.LaunchBrowser();
//        }

//        [When(@"User enters the Username ""([^""]*)"" and Password ""([^""]*)""")]
//        public void WhenUserEntersTheUsernameAndPassword(string username, string password)
//        {
//            loginPage.EnterUsernameAndPassword(username, password);
//        }

//        [When(@"User clicks on login button")]
//        public void WhenUserClicksOnLoginButton()
//        {
//            loginPage.ClickLogin(); // Navigates to AddToCart Page
//        }

//        [Then(@"User is navigated to home page")]
//        public void ThenUserIsNavigatedToHomePage()
//        {
//            Console.WriteLine("User successfully logged in.");
//        }
//    }
//}



//using System;
//using OpenQA.Selenium;
//using SwagProject.Pages;
//using TechTalk.SpecFlow;

//namespace SwagProject.StepDefinitions
//{
//    [Binding]
//    public class LoginStepDefinitions
//    {
//        //private readonly ScenarioContext scenarioContext;
//        //IWebDriver driver;
//        LoginPage lp;

//        public LoginStepDefinitions()
//        {
//            /*scenarioContext = scenarioContext;
//            driver = scenarioContext["WebDriver"] as IWebDriver;*/
//            this.lp = new LoginPage();
//        }
//        [Given(@"User is on swag login page")]
//        public void GivenUserIsOnSwagLoginPage()
//        {
//            lp.launchbrowser();
//            Thread.Sleep(1000);
//        }

//        [When(@"User enters the Username ""([^""]*)"" and Password ""([^""]*)""")]
//        public void WhenUserEntersTheUsernameAndPassword(string username, string password)
//        {
//            lp.enterusernamepass(username, password);
//            Thread.Sleep(1000);
//        }

//        [When(@"User clicks on login button")]
//        public void WhenUserClicksOnLoginButton()
//        {
//            lp.login();
//            Thread.Sleep(1000);
//        }

//        [Then(@"User is navigated to home page")]
//        public void ThenUserIsNavigatedToHomePage()
//        {
//            lp.homepagedisplay();
//            Thread.Sleep(1000);
//        }
//    }
//}


using System;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using SwagProject.Pages;
using TechTalk.SpecFlow;

namespace SwagProject.StepDefinitions
{
    [Binding]
    public class LoginStepDefinitions
    {
        private readonly IWebDriver driver;
        private readonly LoginPage lp;

        public LoginStepDefinitions(ScenarioContext scenarioContext)
        {
            if (!scenarioContext.ContainsKey("WebDriver"))
            {
                throw new Exception("WebDriver is not initialized. Ensure [BeforeScenario] in Hooks runs first.");
            }

            driver = scenarioContext["WebDriver"] as IWebDriver;
            lp = new LoginPage(driver);
        }

        [Given(@"User is on swag login page")]
        public void GivenUserIsOnSwagLoginPage()
        {
            lp.LaunchBrowser();
        }

        [When(@"User enters ""([^""]*)"" and ""([^""]*)""")]
        public void WhenUserEntersUsernameAndPassword(string username, string password)
        {
            lp.EnterUsernameAndPassword(username, password);
        }

        [When(@"User clicks on login button")]
        public void WhenUserClicksOnLoginButton()
        {
            lp.Login();
        }

        [Then(@"User is navigated to home page")]
        public void ThenUserIsNavigatedToHomePage()
        {
            WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
            try
            {
                wait.Until(ExpectedConditions.ElementIsVisible(By.XPath("//div[@class='app_logo']")));
                Console.WriteLine("✅ User successfully logged in.");
            }
            catch (WebDriverTimeoutException)
            {
                throw new Exception("❌ Home page not displayed. Login might have failed.");
            }
        }
    }
}
