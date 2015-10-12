Feature: AppSettings

Scenario: application settings can be mocked as a sanity check
Given I have application settings
    | Key  | Value  |
    | key1 | value1 |
When I load the application settings
Then I should have application settings
    | Key  | Value  |
    | key1 | value1 |