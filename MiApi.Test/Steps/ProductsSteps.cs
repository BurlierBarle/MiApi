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
using System.Net;
using System;

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
            _response = await _client.GetAsync("/api/v1/products");

            _response.EnsureSuccessStatusCode();

            var jsonString = await _response.Content.ReadAsStringAsync();

            var products = JsonConvert.DeserializeObject<List<ProductDto>>(jsonString);

            if (products == null || !products.Any())
            {
                var categoryDto = new CategoryDto
                {
                    Name = "Default Product Category",
                    Description = "Sample description",
                    ProductIds = new List<int>()
                };

                _response = await _client.PostAsJsonAsync("/api/v1/categories", categoryDto);
                _response.EnsureSuccessStatusCode();

                _response = await _client.GetAsync($"/api/v1/categories/Default Product Category");

                var createdCategoryContent = await _response.Content.ReadAsStringAsync();
                var createdCategory = JsonConvert.DeserializeObject<CategoryDto>(createdCategoryContent);
                var categoryId = createdCategory.Id;

                var productDto = new ProductDto
                {
                    Name = "Default Product",
                    Description = "Sample description",
                    CategoryIds = new List<int> { categoryId }
                };

                await _client.PostAsJsonAsync("/api/v1/products", productDto);

                _response.EnsureSuccessStatusCode();
            }
        }

        [Given(@"I have data to view a product with name ""(.*)""")]
        public async Task GivenIHaveDataToViewAProductWithName(string name)
        {
            _response = await _client.GetAsync($"/api/v1/products/{name}");

            if (_response.StatusCode == HttpStatusCode.NotFound)
            {
                _response = await _client.GetAsync("/api/v1/categories/Books");

                var categoryDto = new CategoryDto
                {
                    Name = "Books",
                    Description = "Sample description",
                    ProductIds = []
                };

                _response = await _client.PostAsJsonAsync("/api/v1/categories", categoryDto);
                _response.EnsureSuccessStatusCode();

                _response = await _client.GetAsync("/api/v1/categories/Books");
                var createdCategoryContent = await _response.Content.ReadAsStringAsync();
                var createdCategory = JsonConvert.DeserializeObject<CategoryDto>(createdCategoryContent);
                var categoryId = createdCategory.Id;

                var productDto = new ProductDto
                {
                    Name = name,
                    Description = "Sample description",
                    CategoryIds = new List<int> { categoryId }
                };

                await _client.PostAsJsonAsync("/api/v1/products", productDto);

            }

            _productDto = new ProductDto
            {
                Name = name
            };
        }

        [Given(@"I have data to create a product with name ""(.*)"" in the category ""(.*)""")]
        public async Task GivenIHaveDataToCreateAProductWithNameInTheCategoryId(string name, string categoryName)
        {
            _response = await _client.GetAsync($"/api/v1/categories/{categoryName}");

            if (_response.StatusCode == HttpStatusCode.NotFound)
            {
                var categoryDto = new CategoryDto
                {
                    Name = categoryName,
                    Description = "Sample description",
                    ProductIds = []
                };

                _response = await _client.PostAsJsonAsync("/api/v1/categories", categoryDto);
                _response.EnsureSuccessStatusCode();

                _response = await _client.GetAsync($"/api/v1/categories/{categoryName}");

                var createdCategoryContent = await _response.Content.ReadAsStringAsync();
                var createdCategory = JsonConvert.DeserializeObject<CategoryDto>(createdCategoryContent);
                var categoryId = createdCategory.Id;

                var productDto = new ProductDto
                {
                    Name = name,
                    Description = "Sample description",
                    CategoryIds = new List<int> { categoryId }
                };

                await _client.PostAsJsonAsync("/api/v1/products", productDto);

                _response.EnsureSuccessStatusCode();
            }

            _response = await _client.GetAsync($"/api/v1/products/{name}");

            if (_response.StatusCode != HttpStatusCode.NotFound)
            {
                _response = await _client.DeleteAsync($"/api/v1/products/{name}");
                _response.EnsureSuccessStatusCode();
            }

            _response = await _client.GetAsync($"/api/v1/categories/{categoryName}");
            var createdCategoryProductContent = await _response.Content.ReadAsStringAsync();
            var createdCategoryProduct = JsonConvert.DeserializeObject<CategoryDto>(createdCategoryProductContent);
            var categoryproductId = createdCategoryProduct.Id;

            _productDto = new ProductDto
            {
                Name = name,
                Description = "Sample description",
                CategoryIds = [categoryproductId]
            };

            _response.EnsureSuccessStatusCode();
        }

        [Given(@"I have data to create a product with name ""(.*)"" is taken in the category ""(.*)""")]
        public async Task GivenIHaveDataToCreateAProductWithNameIsTakenInTheCategoryId(string name, string categoryName)
        {
            _response = await _client.GetAsync($"/api/v1/products/{name}");

            if (_response.StatusCode == HttpStatusCode.NotFound)
            {
                var categoryDto = new CategoryDto
                {
                    Name = categoryName,
                    Description = "Sample description",
                    ProductIds = []
                };

                _response = await _client.PostAsJsonAsync("/api/v1/categories", categoryDto);
                _response.EnsureSuccessStatusCode();

                _response = await _client.GetAsync($"/api/v1/categories/{categoryName}");

                var createdCategoryContent = await _response.Content.ReadAsStringAsync();
                var createdCategory = JsonConvert.DeserializeObject<CategoryDto>(createdCategoryContent);
                var categoryId = createdCategory.Id;

                var productDto = new ProductDto
                {
                    Name = name,
                    Description = "Sample description",
                    CategoryIds = new List<int> { categoryId }
                };

                await _client.PostAsJsonAsync("/api/v1/products", productDto);

                _response.EnsureSuccessStatusCode();
            }

            _response = await _client.GetAsync($"/api/v1/categories/{categoryName}");
            var createdCategoryProductContent = await _response.Content.ReadAsStringAsync();
            var createdCategoryProduct = JsonConvert.DeserializeObject<CategoryDto>(createdCategoryProductContent);
            var categoryproductId = createdCategoryProduct.Id;

            _productDto = new ProductDto
            {
                Name = name,
                Description = "Sample description",
                CategoryIds = [categoryproductId]
            };

            _response.EnsureSuccessStatusCode();
        }

        [Given(@"I have data to delete a product with name ""(.*)""")]
        public async Task GivenIHaveDataToDeleteAProductWithName(string name)
        {
            _response = await _client.GetAsync($"/api/v1/products/{name}");

            if (_response.StatusCode == HttpStatusCode.NotFound)
            {
                
                _response = await _client.GetAsync($"/api/v1/categories/Pets");

                var createdCategoryContent = await _response.Content.ReadAsStringAsync();
                var createdCategory = JsonConvert.DeserializeObject<CategoryDto>(createdCategoryContent);
                var categoryId = createdCategory.Id;

                var productDto = new ProductDto
                {
                    Name = name,
                    Description = "Sample description",
                    CategoryIds = new List<int> { categoryId }
                };

                await _client.PostAsJsonAsync("/api/v1/products", productDto);

                _response.EnsureSuccessStatusCode();
            }

            _productDto = new ProductDto
            {
                Name = name
            };
        }

        [Given(@"I have data to edit a product with name ""(.*)""")]
        public async Task GivenIHaveDataToEditAProductWithName(string name)
        {
            _response = await _client.GetAsync($"/api/v1/products/{name}");

            if (_response.StatusCode == HttpStatusCode.NotFound)
            {
                _response = await _client.GetAsync("/api/v1/categories/Consoles");

                if (_response.StatusCode == HttpStatusCode.NotFound)
                {
                    var categoryDto = new CategoryDto
                    {
                        Name = "Consoles",
                        Description = "Sample description",
                        ProductIds = []
                    };

                    _response = await _client.PostAsJsonAsync("/api/v1/categories", categoryDto);
                    _response.EnsureSuccessStatusCode();
                }

                _response = await _client.GetAsync("/api/v1/categories/Consoles");
                var createdCategoryContent = await _response.Content.ReadAsStringAsync();
                var createdCategory = JsonConvert.DeserializeObject<CategoryDto>(createdCategoryContent);
                var categoryId = createdCategory.Id;

                var productDto = new ProductDto
                {
                    Name = name,
                    Description = "Sample description",
                    CategoryIds = new List<int> { categoryId }
                };

                await _client.PostAsJsonAsync("/api/v1/products", productDto);

                _response.EnsureSuccessStatusCode();
            }

            _productDto = new ProductDto
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
            _response = await _client.GetAsync("/api/v1/categories/Consoles");
            var createdCategoryContent = await _response.Content.ReadAsStringAsync();
            var createdCategory = JsonConvert.DeserializeObject<CategoryDto>(createdCategoryContent);
            var categoryId = createdCategory.Id;

            var updatedProduct = new ProductDto
            {
                Name = name,
                Description = description,
                CategoryIds = [categoryId]
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
        public async Task ThenTheProductShouldBeCreatedInTheDatabase()
        {
            _response = await _client.GetAsync("/api/v1/products");

            _response.EnsureSuccessStatusCode();

            var jsonString = await _response.Content.ReadAsStringAsync();

            var products = JsonConvert.DeserializeObject<List<CategoryDto>>(jsonString);

            var createdProducts = products.FirstOrDefault(c => c.Name == _productDto.Name);

            createdProducts.Should().NotBeNull("Expected products to be created, but it was not found in the database.");
        }

        [Then(@"The response should get an error message that the name is already in use")]
        public async Task ThentTheResponseShouldGetAnErrorMessageThatTheNameIsAlreadyInUse()
        {
            var content = await _response.Content.ReadAsStringAsync();
            content.Should().Contain("The name is already in use");
        }

        [Then(@"The response should have updated the product ""(.*)"" in the database")]
        public async Task ThenResponseShouldProductUpdatedInTheDataBase(string name)
        {
            _response = await _client.GetAsync($"/api/v1/products?name={name}");

            _response.EnsureSuccessStatusCode();

            var jsonString = await _response.Content.ReadAsStringAsync();
            var products = JsonConvert.DeserializeObject<List<ProductDto>>(jsonString);

            var updatedProduct = products.FirstOrDefault(c => c.Name == name);

            updatedProduct.Should().NotBeNull();
            updatedProduct.Description.Should().Be("Xbox One");
        }

        [Then(@"The product ""(.*)"" should not exist in the database")]
        public async Task ThenTheProductShouldNotExistInTheDatabase(string name)
        {
            _response = await _client.GetAsync($"/api/v1/product/{name}");

            _response.StatusCode.Should().Be(HttpStatusCode.NotFound, "Product should not exist");
        }

        [Then(@"The response should get an error message that the name product field is required")]
        public async Task ThenTheResponseShouldGetAnErrorMessageThatTheNameProductFieldIsRequired()
        {
            var content = await _response.Content.ReadAsStringAsync();
            content.Should().Contain("The Name field is required.");
        }
    }
}
