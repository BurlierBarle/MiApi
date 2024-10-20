Feature: Category Management

Scenario: User can view the list of categories
    Given the database has categories
    When I send a GET request to "/api/v1/categories" 
    Then the response status should be 200
    And the response should contain a list of categories

Scenario: User can view each of categories
    Given I have data to view a category with name "Soda"
    When I send a GET request to "/api/v1/categories/Soda" with the category data
    Then the response status should be 200
    And the response should contain a category 

Scenario: User can create a category
    Given I have data to create a category with name "Phones"
    When I send a POST request to "/api/v1/categories" with the category data
    Then the response status should be 201
    And the category should be created in the database

Scenario: User tries to create a category but the name is already taken
    Given I have data to create a category with name "Pets"
    When I send a POST request to "/api/v1/categories" with the category data
    Then the response status should be 422
    And the response should get an error message that the name is already in use

Scenario: User can edit category
    Given I have data to edit a category with name "Soda"
    When I send a PUT to "/api/v1/categories/Soda" with the name "Soda" and the description "Soda Free"
    Then the response status should be 204
    And The response should have updated the category in the database

Scenario: User tries to edit category with empty name
    Given I have data to edit a category with name "Soda"
    When I send a PUT to "/api/v1/categories/Soda" with empty name
    Then the response status should be 400
    And The response should get an error message that the name field is required

Scenario: User can destroy a category
    Given I have data to delete a category with name "Phones"
    When I send a DELETE to "/api/v1/categories/Phones"
    Then the response status should be 204
    And The category should not exist in the database

Scenario: User tries to destroy a category but it has products assigned
    Given I have a category with name "Computers" that has products
    When I send a DELETE to "/api/v1/categories/Computers"
    Then the response status should be 422
    And The response should get an error message that cannot delete category with assigned products.

Scenario: User deletes products and then deletes the category
    Given I have a category with name "Fruit" that has products to delete
    And I delete all products from the category name "Fruit"
    When I send a DELETE to "/api/v1/categories/Fruit"
    Then the response status should be 204
    And The category should not exist in the database