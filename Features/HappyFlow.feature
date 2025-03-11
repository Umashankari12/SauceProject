Feature: HappyFlow

A short summary of the feature

Scenario: login with valid credentials
	Given User is on swag login page
	When User enters the Username "<standard_user>" and Password "<secret_sauce>"
    And User clicks on login button
	Then User is navigated to home page

	When User clicks the products
	And User clicks add to cart button
	And User clicks Cart Icon
	Then Item added to cart should display

	When User clicks checkout button
	And User enters "<uma>", "<shankari>", and "<123456>"
	Then Then Clicks on Continue

	When User clicks on Finish
    Then Order status should be visible
	


