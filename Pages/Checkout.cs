//using OpenQA.Selenium;

//namespace SwagProject.Pages
//{
//    internal class Checkout
//    {
//        private readonly IWebDriver driver;

//        public Checkout(IWebDriver driver)
//        {
//            this.driver = driver;
//        }

//        // Locators
//        private readonly By checkoutBtn = By.XPath("//button[@id='checkout']");
//        private readonly By firstNameField = By.XPath("//input[@id='first-name']");
//        private readonly By lastNameField = By.XPath("//input[@id='last-name']");
//        private readonly By zipCodeField = By.XPath("//input[@id='postal-code']");
//        private readonly By continueButton = By.XPath("//input[@id='continue']");

//        public Checkout ClickCheckout()
//        {
//            driver.FindElement(checkoutBtn).Click();
//            return this;
//        }

//        public Checkout EnterDetails(string firstName, string lastName, string zipCode)
//        {
//            driver.FindElement(firstNameField).SendKeys(firstName);
//            driver.FindElement(lastNameField).SendKeys(lastName);
//            driver.FindElement(zipCodeField).SendKeys(zipCode);
//            return this;
//        }

//        public Overview ClickContinue()
//        {
//            driver.FindElement(continueButton).Click();
//            return new Overview(driver);
//        }
//    }
//}


using OpenQA.Selenium;
using SwagProject.Locators;

namespace SwagProject.Pages
{
    public class Checkout
    {
        private readonly IWebDriver driver;

        public Checkout(IWebDriver driver)
        {
            this.driver = driver;
        }

        public void CheckoutProcess()
        {
            driver.FindElement(check.checkoutbtn).Click();
        }

        public void EnterDetails(string firstname, string lastname, string zipcode)
        {
            driver.FindElement(check.checkOutFirstName).SendKeys(firstname);
            driver.FindElement(check.checkOutLastName).SendKeys(lastname);
            driver.FindElement(check.checkOutPostalCode).SendKeys(zipcode);
        }

        public void ContinueCheckout()
        {
            driver.FindElement(check.continueCheckOut).Click();
        }
    }
}
