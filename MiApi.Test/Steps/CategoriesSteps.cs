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
    public class CategoriesSteps
    {
        private readonly ScenarioContext _scenarioContext;
        private readonly HttpClient _client;
        private HttpResponseMessage _response;
        private CategoryDto? _categoryDto;
        private int _categoryId;

        public CategoriesSteps(ScenarioContext scenarioContext)
        {
            _scenarioContext = scenarioContext; 
            var appFactory = new WebApplicationFactory<Program>();
            _client = appFactory.CreateClient();
        }

        [Given(@"the database has categories")]
        public async Task GivenTheDatabaseHasCategories()
        {
            // Seed the database with categories if necessary
        }


        [Given(@"I have data to create a category with name ""(.*)""")]
        public void GivenIHaveDataToCreateACategoryWithName(string name)
        {
            _categoryDto = new CategoryDto
            {
                Name = name,
                Description = "Sample description",
                ProductIds = []
            };
        }

        [Given(@"I have data to view a category with name ""(.*)""")]
        public void GivenIHaveDataToViewACategoryWithName(string name)
        {
            _categoryDto = new CategoryDto
            {
                Name = name
            };

        }

        [Given(@"I have data to edit a category with name ""(.*)""")]
        public void GivenIHaveDataToEditACategoryWithName(string name)
        {
            var categoryDto = new CategoryDto
            {
                Name = name
            };
        }

        [Given(@"I have data to delete a category with name ""(.*)""")]
        public void GivenIHaveDataToDeleteACategoryWithName(string name)
        {
            var categoryDto = new CategoryDto
            {
                Name = name
            };
        }

        [Given(@"I have a category with name ""(.*)"" that has products")]
        public void GivenIHaveCategoryWithNameThatHasProducts(string name)
        {
            var categoryDto = new CategoryDto
            {
                Name = name
            };

        }

        [Given(@"I have a category with name ""(.*)"" that has products to delete")]
        public async Task GivenIHaveCategoryWithNameThatHasProductsToDelete(string name)
        {
            // Crea la categoría si no existe
            var categoryDto = new CategoryDto
            {
                Name = name,
                Description = "Description for fruits",
                ProductIds = []
            };

            var postResponse = await _client.PostAsJsonAsync("/api/v1/categories", categoryDto);
            postResponse.EnsureSuccessStatusCode();

            // Obtén el ID de la categoría recién creada
            var createdCategoryContent = await postResponse.Content.ReadAsStringAsync();
            var createdCategory = JsonConvert.DeserializeObject<CategoryDto>(createdCategoryContent);
            _categoryId = createdCategory.Id; // Guarda el ID de la categoría

            // Crea un producto asociado a la categoría
            var productDto = new ProductDto
            {
                Name = "Apple",
                Description = "A juicy fruit",
                CategoryIds = new List<int> { _categoryId } // Usa el ID de la categoría recién creada
            };

            await _client.PostAsJsonAsync("/api/v1/products", productDto);

        }

        [Given(@"I delete all products from the category name ""(.*)""")]
        public async Task GivenIDeleteAllProductsFromTheCategoryName(string categoryName)
        {
            var categoriesResponse = await _client.GetAsync("/api/v1/categories");
            var categories = JsonConvert.DeserializeObject<List<CategoryDto>>(await categoriesResponse.Content.ReadAsStringAsync());

            // Buscar la categoría por nombre y obtener su ID
            var category = categories.FirstOrDefault(c => c.Name == categoryName);

            // Si la categoría no se encuentra, no hay nada que eliminar
            if (category == null)
            {
                throw new Exception($"Category not founded");
            }

            // Obtener la lista de productos
            var productsResponse = await _client.GetAsync("/api/v1/products");
            var productList = JsonConvert.DeserializeObject<List<ProductDto>>(await productsResponse.Content.ReadAsStringAsync());

            // Eliminar productos que pertenecen a la categoría encontrada
            foreach (var product in productList)
            {
                if (product.CategoryIds.Contains(category.Id))
                {
                    var deleteResponse = await _client.DeleteAsync($"/api/v1/products/{product.Name}"); // Cambia a usar nombre
                    deleteResponse.EnsureSuccessStatusCode(); // Asegúrate de que la eliminación fue exitosa
                }
            }
        }

        [When(@"I send a POST request to ""(.*)"" with the category data")]
        public async Task WhenISendAPOSTRequestToWithTheCategoryData(string url)
        {
            _response = await _client.PostAsJsonAsync(url, _categoryDto);
        }

        [When(@"I send a GET request to ""(.*)"" with the category data")]
        public async Task WhenISendAGETRequestToWithTheCategoryData(string url)
        {
            _response = await _client.GetAsync(url);
        }

        [When(@"I send a GET request to ""(.*)""")]
        public async Task WhenISendAGETRequestTo(string url)
        {
            _response = await _client.GetAsync(url);
        }

        [When(@"I send a PUT to ""(.*)"" with the name ""(.*)"" and the description ""(.*)""")]
        public async Task WhenISendAPutToWithTheNameAndDescription(string url, string name, string description)
        {
            var updatedCategory = new CategoryDto
            {
                Name = name, 
                Description = description,
                ProductIds = []
            };

            _response = await _client.PutAsJsonAsync(url, updatedCategory);
        }

        [When(@"I send a PUT to ""(.*)"" with empty name")]
        public async Task WhenISendAPutToWithEmptyName(string url)
        {
            var emptyCategoryDto = new CategoryDto
            {
                Name = null,
                Description = "null"
            };

            _response = await _client.PutAsJsonAsync(url, emptyCategoryDto);
        }

        [When(@"I send a DELETE to ""(.*)""")]
        public async Task WhenISendADeleteTo(string url)
        {
            // Enviar la petición DELETE
            _response = await _client.DeleteAsync(url);
        }

        [Then(@"the response status should be (.*)")]
        public void ThenTheResponseStatusShouldBe(int statusCode)
        {
            ((int)_response.StatusCode).Should().Be(statusCode);
        }

        [Then(@"the response should contain a list of categories")]
        public async Task ThenTheResponseShouldContainAListOfCategories()
        {
            var content = await _response.Content.ReadAsStringAsync();
            content.Should().NotBeNullOrEmpty();
        }

        [Then(@"the response should contain a category")]
        public async Task ThenTheResponseShouldContainACategory()
        {
            var content = await _response.Content.ReadAsStringAsync();
            content.Should().NotBeNullOrEmpty();
        }

        [Then(@"the category should be created in the database")]
        public void ThenTheCategoryShouldBeCreatedInTheDatabase()
        {
            // Verify that the category exists in the database
        }

        [Then(@"The response should have updated the category in the database")]
        public void ThenResponseShouldUpdatedInTheDataBase()
        {
            // Verify that the category exists in the database
        }

        [Then(@"the response should get an error message that the name is already in use")]
        public async Task ThenTheResponseShouldGetAnErrorMessageThatTheNameIsAlreadyInUse()
        {
            var content = await _response.Content.ReadAsStringAsync();
            content.Should().Contain("The name is already in use."); 
        }

        [Then(@"The response should get an error message that the name field is required")]
        public async Task ThenTheResponseShouldGetAnErrorMessageThatTheNameFieldIsRequired()
        {
            var content = await _response.Content.ReadAsStringAsync();
            content.Should().Contain("The Name field is required.");
        }

        [Then(@"The category should not exist in the database")]
        public void ThenTheCategoryShouldNotExistInTheDatabase()
        {
            // Verify that the category does not exist in the database
        }

        [Then(@"The response should get an error message that cannot delete category with assigned products.")]
        public async Task ThenTheResponseShouldGetAnErrorMessageThatCannotDeleteCategoryWithAssignedProducts()
        {
            var content = await _response.Content.ReadAsStringAsync();
            content.Should().Contain("Cannot delete category with assigned products.");
        }

    }
}
