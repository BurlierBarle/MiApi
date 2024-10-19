Feature: Category Management

Scenario: User can create a category
    Given I have data to create a category with name "Beverages"
    When I send a POST request to "/api/v1/categories" with the category data
    Then the response status should be 201
    And the category should be created in the database

