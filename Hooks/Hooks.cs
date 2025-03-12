// using NUnit.Framework;
// using OpenQA.Selenium;
// using TechTalk.SpecFlow;
// using WebDriverManager.DriverConfigs.Impl;
// using WebDriverManager;
// using AventStack.ExtentReports.Reporter;
// using AventStack.ExtentReports;
// using System;
// using System.IO;
// using System.Threading;
// using OpenQA.Selenium.Chrome;

// namespace SwagProject.Hooks
// {
//     [Binding]
//     public class Hooks
//     {
//         public static IWebDriver driver;
//         private readonly ScenarioContext _scenarioContext;
//         private static ExtentReports _extent;
//         private static ExtentTest _feature;
//         private ExtentTest _scenario;
//         private static ExtentSparkReporter _sparkReporter;

//         public Hooks(ScenarioContext scenarioContext)
//         {
//             _scenarioContext = scenarioContext;
//         }

//         [BeforeTestRun]
//         public static void BeforeTestRun()
//         {
//             string reportDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Artifacts", "Reports");
//             string reportPath = Path.Combine(reportDirectory, "ExtentReport.html");
//             Directory.CreateDirectory(reportDirectory);

//             _sparkReporter = new ExtentSparkReporter(reportPath);
//             _extent = new ExtentReports();
//             _extent.AttachReporter(_sparkReporter);
//         }

//         [BeforeFeature]
//         public static void BeforeFeature(FeatureContext featureContext)
//         {
//             _feature = _extent.CreateTest(featureContext.FeatureInfo.Title);
//         }

//         [BeforeScenario]
//         public void Setup()
//         {
//             TestContext.Progress.WriteLine("Initializing WebDriver in headless mode...");

//             new DriverManager().SetUpDriver(new ChromeConfig());
//             ChromeOptions chromeOptions = new ChromeOptions();
            
//             // Headless Mode & Additional Arguments for CI Environments
//             chromeOptions.AddArgument("--headless=new");
//             chromeOptions.AddArgument("--disable-gpu");
//             chromeOptions.AddArgument("--no-sandbox");
//             chromeOptions.AddArgument("--window-size=1920,1080");

//             // Set Chrome Binary Path explicitly
//             chromeOptions.BinaryLocation = "/usr/bin/google-chrome";

//             driver = new ChromeDriver(chromeOptions);
            
//             _scenarioContext["WebDriver"] = driver;
//             _scenario = _feature.CreateNode(_scenarioContext.ScenarioInfo.Title);
//         }

//         [AfterScenario]
//         public void TearDown()
//         {
//             if (driver != null)
//             {
//                 driver.Quit();
//                 driver.Dispose();
//                 driver = null;
//             }
//         }

//         [AfterTestRun]
//         public static void AfterTestRun()
//         {
//             _extent.Flush();
//         }
//     }
// }


using NUnit.Framework;
using OpenQA.Selenium;
using TechTalk.SpecFlow;
using WebDriverManager;
using AventStack.ExtentReports.Reporter;
using AventStack.ExtentReports;
using System;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Threading;
using OpenQA.Selenium.Chrome;

namespace SwagProject.Hooks
{
    [Binding]
    public class Hooks
    {
        public static IWebDriver driver;
        private readonly ScenarioContext _scenarioContext;
        private static ExtentReports _extent;
        private static ExtentTest _feature;
        private ExtentTest _scenario;
        private static ExtentSparkReporter _sparkReporter;

        public Hooks(ScenarioContext scenarioContext)
        {
            _scenarioContext = scenarioContext;
        }

        [BeforeTestRun]
        public static void BeforeTestRun()
        {
            string reportDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Reports");
            string reportPath = Path.Combine(reportDirectory, "ExtentReport.html");

            Directory.CreateDirectory(reportDirectory);

            _sparkReporter = new ExtentSparkReporter(reportPath);
            _extent = new ExtentReports();
            _extent.AttachReporter(_sparkReporter);
        }

        [BeforeFeature]
        public static void BeforeFeature(FeatureContext featureContext)
        {
            _feature = _extent.CreateTest(featureContext.FeatureInfo.Title);
        }

        [BeforeScenario]
        public void Setup()
        {
            TestContext.Progress.WriteLine("Initializing WebDriver...");

            if (driver == null)
            {
                ChromeOptions options = new ChromeOptions();
                options.AddArgument("--headless");
                options.AddArgument("--no-sandbox");
                options.AddArgument("--disable-dev-shm-usage");
                options.AddArgument("--remote-debugging-port=9222");
                options.AddArgument($"--user-data-dir=/tmp/chrome-profile-{Guid.NewGuid()}");

                driver = new ChromeDriver(options);
            }

            _scenarioContext["WebDriver"] = driver;
            _scenario = _feature.CreateNode(_scenarioContext.ScenarioInfo.Title);
        }

        [AfterScenario]
        public void TearDown()
        {
            if (driver != null)
            {
                driver.Quit();
                driver = null;
            }
        }

        [AfterTestRun]
        public static void AfterTestRun()
        {
            _extent.Flush();
            SendEmailWithGmail();
        }

        private static void SendEmailWithGmail()
        {
            try
            {
                string smtpServer = "smtp.gmail.com";
                int smtpPort = 587;
                string senderEmail = "shankariu804@gmail.com";
                string senderPassword = "exry tjbv yrxb ctnu";
                string recipientEmail = "shankariu8@gmail.com";

                MailMessage mail = new MailMessage
                {
                    From = new MailAddress(senderEmail),
                    Subject = "SpecFlow Test Report & Screenshots",
                    Body = "Attached are the Extent Report and failure screenshots from the latest test execution.",
                    IsBodyHtml = false
                };

                mail.To.Add(recipientEmail);

                string reportPath = Path.Combine(Directory.GetCurrentDirectory(), "Reports", "ExtentReport.html");
                if (File.Exists(reportPath))
                    mail.Attachments.Add(new Attachment(reportPath));

                SmtpClient smtp = new SmtpClient(smtpServer, smtpPort)
                {
                    Credentials = new NetworkCredential(senderEmail, senderPassword),
                    EnableSsl = true
                };

                smtp.Send(mail);
                TestContext.Progress.WriteLine("✅ Email sent successfully via Gmail SMTP!");
            }
            catch (Exception ex)
            {
                TestContext.Progress.WriteLine($"❌ Failed to send email via Gmail SMTP: {ex.Message}");
            }
        }
    }
}
