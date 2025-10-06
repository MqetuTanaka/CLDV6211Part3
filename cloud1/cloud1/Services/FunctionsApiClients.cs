using cloud1.Models;
using System.Text;
using System.Text.Json;

namespace cloud1.Services
{
    public class FunctionsApiService : IFunctionsApi
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<FunctionsApiService> _logger;
        private readonly JsonSerializerOptions _jsonOptions;

        public FunctionsApiService(HttpClient httpClient, ILogger<FunctionsApiService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                PropertyNameCaseInsensitive = true
            };

            // Set timeout and other HttpClient settings
            _httpClient.Timeout = TimeSpan.FromSeconds(30);
        }

        public async Task<List<Customer>> GetCustomersAsync()
        {
            try
            {
                _logger.LogInformation("Calling Azure Function to get customers");
                var response = await _httpClient.GetAsync("GetCustomers");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    _logger.LogInformation($"Received response: {content}");

                    var customers = JsonSerializer.Deserialize<List<Customer>>(content, _jsonOptions);
                    _logger.LogInformation($"Deserialized {customers?.Count ?? 0} customers");
                    return customers ?? new List<Customer>();
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError($"Failed to get customers. Status: {response.StatusCode}, Error: {errorContent}");
                    return new List<Customer>();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting customers from Azure Function");
                return new List<Customer>();
            }
        }

        public async Task<Customer> GetCustomerAsync(string id)
        {
            try
            {
                _logger.LogInformation($"Calling Azure Function to get customer {id}");
                var response = await _httpClient.GetAsync($"GetCustomer?id={id}");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    return JsonSerializer.Deserialize<Customer>(content, _jsonOptions);
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError($"Failed to get customer {id}. Status: {response.StatusCode}, Error: {errorContent}");
                    return null;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting customer {id} from Azure Function");
                return null;
            }
        }

        public async Task CreateCustomerAsync(Customer customer)
        {
            try
            {
                _logger.LogInformation($"Calling Azure Function to create customer {customer.CustomerID}");

                var json = JsonSerializer.Serialize(customer, _jsonOptions);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync("CreateCustomer", content);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError($"Failed to create customer. Status: {response.StatusCode}, Error: {errorContent}");
                    throw new HttpRequestException($"Azure Function returned {response.StatusCode}: {errorContent}");
                }

                _logger.LogInformation("Customer created successfully via Azure Function");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating customer via Azure Function");
                throw;
            }
        }

        public async Task UpdateCustomerAsync(string id, Customer customer)
        {
            try
            {
                _logger.LogInformation($"Calling Azure Function to update customer {id}");

                var json = JsonSerializer.Serialize(customer, _jsonOptions);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PutAsync($"UpdateCustomer?id={id}", content);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError($"Failed to update customer {id}. Status: {response.StatusCode}, Error: {errorContent}");
                    throw new HttpRequestException($"Azure Function returned {response.StatusCode}: {errorContent}");
                }

                _logger.LogInformation("Customer updated successfully via Azure Function");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating customer {id} via Azure Function");
                throw;
            }
        }

        public async Task DeleteCustomerAsync(string id)
        {
            try
            {
                _logger.LogInformation($"Calling Azure Function to delete customer {id}");

                var response = await _httpClient.DeleteAsync($"DeleteCustomer?id={id}");

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError($"Failed to delete customer {id}. Status: {response.StatusCode}, Error: {errorContent}");
                    throw new HttpRequestException($"Azure Function returned {response.StatusCode}: {errorContent}");
                }

                _logger.LogInformation("Customer deleted successfully via Azure Function");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting customer {id} via Azure Function");
                throw;
            }
        }
    }
}