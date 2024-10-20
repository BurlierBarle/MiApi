using FluentAssertions;
using MiApi.Context;
using MiApi.Dtos;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Xunit;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using System.Net.Http.Json;
using TechTalk.SpecFlow;
using MiApi.Models;
using Newtonsoft.Json;

namespace MiApi.Test.Steps
{
    [Binding]
    public class ProductsSteps
    {
        private readonly ScenarioContext _scenarioContext;
        private readonly HttpClient _client;
        private HttpResponseMessage _response;
        private ProductDto? _productDto;
        public ProductsSteps(ScenarioContext scenarioContext)
        {
            _scenarioContext = scenarioContext;
            var appFactory = new WebApplicationFactory<Program>();
            _client = appFactory.CreateClient();
        }

        [Given(@"the database has products")]
        public async Task GivenTheDatabaseHasProducts()
        {
            // Seed the database with products if necessary
        }

        [Given(@"I have data to view a product with name ""(.*)""")]
        public void GivenIHaveDataToViewAProductWithName(string name)
        {
            _productDto = new ProductDto
            {
                Name = name
            };
        }

        [Given(@"I have data to create a product with name ""(.*)"" in the category id ""(.*)""")]
        public void GivenIHaveDataToCreateAProductWithNameInTheCategoryId(string productName, int categoryId)
        {
            _productDto = new ProductDto
            {
                Name = productName,
                Description = "Sample description",
                CategoryIds = [categoryId]
            };
        }

        [Given(@"I have data to delete a product with name ""(.*)""")]
        public void GivenIHaveDataToDeleteAProductWithName(string name)
        {
            var productDto = new ProductDto
            {
                Name = name
            };
        }

        [Given(@"I have data to edit a product with name ""(.*)""")]
        public void GivenIHaveDataToEditAProductWithName(string name)
        {
            var productDto = new ProductDto
            {
                Name = name
            };
        }

        [When(@"I send a PUT product to ""(.*)"" with empty name")]
        public async Task WhenISendAPutProductToWithEmptyName(string url)
        {
            var emptyProductDto = new ProductDto
            {
                Name = null,
                Description = "null"
            };

            _response = await _client.PutAsJsonAsync(url, emptyProductDto);
        }

        [When(@"I send a GET request to ""(.*)"" with the products data")]
        public async Task WhenISendAGETRequestToWithTheProductsData(string url)
        {
            _response = await _client.GetAsync(url);
        }

        [When(@"I send a GET request products to ""(.*)""")]
        public async Task WhenISendAGETRequestProductsTo(string url)
        {
            _response = await _client.GetAsync(url);
        }

        [When(@"I send a POST request to ""(.*)"" with the product data")]
        public async Task WhenISendAPostRequestToWithTheProductData(string url)
        {
            _response = await _client.PostAsJsonAsync(url, _productDto);
        }

        [When(@"I send a DELETE product to ""(.*)""")]
        public async Task WhenISendADeleteProductTo(string url)
        {
            _response = await _client.DeleteAsync(url);
        }

        [When(@"I send a PUT product to ""(.*)"" with the name ""(.*)"" and the description ""(.*)""")]
        public async Task WhenISendAPutProductToWithTheNameAndDescription(string url, string name, string description)
        {
            var updatedProduct = new ProductDto
            {
                Name = name,
                Description = description,
                CategoryIds = [6]
            };

            _response = await _client.PutAsJsonAsync(url, updatedProduct);
        }

        [Then(@"The response status should be (.*)")]
        public void ThenTheResponseStatusShouldBe(int statusCode)
        {
            ((int)_response.StatusCode).Should().Be(statusCode);
        }

        [Then(@"the response should contain a list of products")]
        public async Task ThenTheResponseShouldContainAListOfProductsWithTheCategories()
        {
            var content = await _response.Content.ReadAsStringAsync();
            content.Should().NotBeNullOrEmpty();
        }

        [Then(@"the response show the list of categories of product")]
        public async Task ThenTheResponseShowTheListOfCategoriesOfProduct()
        {
            var content = await _response.Content.ReadAsStringAsync();
            var products = JsonConvert.DeserializeObject<List<ProductDto>>(content);

            foreach (var product in products)
            {
                product.CategoryIds.Should().NotBeNull(); 
                product.CategoryIds.Should().HaveCountGreaterThan(0); 
            }
        }

        [Then(@"the response show the categories of product")]
        public async Task ThenTheResponseShowTheCategoriesOfProduct()
        {
            var content = await _response.Content.ReadAsStringAsync();
            var product = JsonConvert.DeserializeObject<ProductDto>(content);

            product.CategoryIds.Should().NotBeNull();
            product.CategoryIds.Should().HaveCountGreaterThan(0);
        }

        [Then(@"the product should be created in the database")]
        public void ThenTheProductShouldBeCreatedInTheDatabase()
        {
            // Verify that the product does not exist in the database
        }

        [Then(@"The response should get an error message that the name is already in use")]
        public async Task ThentTheResponseShouldGetAnErrorMessageThatTheNameIsAlreadyInUse()
        {
            var content = await _response.Content.ReadAsStringAsync();
            content.Should().Contain("The name is already in use");
        }

        [Then(@"The response should have updated the product in the database")]
        public void ThenResponseShouldProductUpdatedInTheDataBase()
        {
            // Verify that the product exists in the database
        }

        [Then(@"The product should not exist in the database")]
        public void ThenTheProductShouldNotExistInTheDatabase()
        {
            // Verify that the product does not exist in the database
        }

        [Then(@"The response should get an error message that the name product field is required")]
        public async Task ThenTheResponseShouldGetAnErrorMessageThatTheNameProductFieldIsRequired()
        {
            var content = await _response.Content.ReadAsStringAsync();
            content.Should().Contain("The Name field is required.");
        }
    }
}
