Feature: Product Management

Scenario: User can view the list of products
    Given the database has products
    When I send a GET request products to "/api/v1/products" 
    Then The response status should be 200
    And the response should contain a list of products with the categories

Scenario: User can view each of product
    Given I have data to view a product with name "Asus"
    When I send a GET request to "/api/v1/products/Asus" with the products data
    Then The response status should be 200
    And the response should contain a list of products with the categories

Scenario: User can create a product
    Given I have data to create a product with name "Alienware" in the category id "6"
    When I send a POST request to "/api/v1/products" with the product data
    Then The response status should be 201
    And the product should be created in the database

Scenario: User tries to create a product but the name is already taken category
    Given I have data to create a product with name "Asus" in the category id "6"
    When I send a POST request to "/api/v1/products" with the product data
    Then The response status should be 422
    And The response should get an error message that the name is already in use
    