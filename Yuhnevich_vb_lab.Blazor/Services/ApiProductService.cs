using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using Yuhnevich_vb_lab.Domain.Entities;
using Yuhnevich_vb_lab.Domain.Models;

namespace Yuhnevich_vb_lab.Blazor.Services
{
    public class ApiProductService : IProductService<Dish>
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<ApiProductService> _logger;
        private List<Dish> _dishes = new List<Dish>();
        private int _currentPage = 1;
        private int _totalPages = 1;
        private bool _isFetching = false;

        public IEnumerable<Dish> Products => _dishes;
        public int CurrentPage => _currentPage;
        public int TotalPages => _totalPages;
        public string? ErrorMessage { get; private set; }
        public event Action? ListChanged;

        public ApiProductService(HttpClient httpClient, ILogger<ApiProductService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task GetProducts(int pageNo = 1, int pageSize = 3)
        {
            _logger.LogInformation("GetProducts called with pageNo={PageNo}, pageSize={PageSize}", pageNo, pageSize);
            if (_isFetching)
            {
                _logger.LogInformation("Skipping request, fetch in progress for pageNo={PageNo}, pageSize={PageSize}", pageNo, pageSize);
                return;
            }

            _isFetching = true;
            try
            {
                // Используем альтернативный формат эндпоинта /api/dishes/page/{pageNo}
                var uri = new Uri(_httpClient.BaseAddress, $"api/dishes/page/{pageNo}");
                var queryData = new Dictionary<string, string>
                {
                    { "pageSize", pageSize.ToString() }
                };
                var query = HttpUtility.ParseQueryString(string.Empty);
                foreach (var pair in queryData)
                {
                    query[pair.Key] = pair.Value;
                }
                var fullUri = $"{uri}?{query}";
                _logger.LogInformation("Sending GET request to {Uri}", fullUri);
                var result = await _httpClient.GetAsync(fullUri);

                if (result.IsSuccessStatusCode)
                {
                    var responseData = await result.Content.ReadFromJsonAsync<ResponseData<ListModel<Dish>>>();
                    _logger.LogInformation("API response: Success={Success}, ItemsCount={Count}, CurrentPage={CurrentPage}, TotalPages={TotalPages}, Error={Error}",
                        responseData?.Success, responseData?.Data?.Items?.Count, responseData?.Data?.CurrentPage, responseData?.Data?.TotalPages, responseData?.ErrorMessage);

                    if (responseData?.Success == true && responseData.Data != null)
                    {
                        _currentPage = responseData.Data.CurrentPage;
                        _totalPages = responseData.Data.TotalPages;
                        _dishes = responseData.Data.Items ?? new List<Dish>();
                        ErrorMessage = null;
                        _logger.LogInformation("Products loaded: {Products}", string.Join(", ", _dishes.Select(d => d.Name)));
                    }
                    else
                    {
                        _dishes = new List<Dish>();
                        _currentPage = 1;
                        _totalPages = 1;
                        ErrorMessage = responseData?.ErrorMessage ?? "API returned unsuccessful response or null data";
                        _logger.LogWarning("API returned unsuccessful response or null data: {Error}", responseData?.ErrorMessage);
                    }
                }
                else
                {
                    _dishes = new List<Dish>();
                    _currentPage = 1;
                    _totalPages = 1;
                    ErrorMessage = $"API request failed with status code {result.StatusCode}";
                    _logger.LogError("API request failed with status code {StatusCode}", result.StatusCode);
                }
            }
            catch (Exception ex)
            {
                _dishes = new List<Dish>();
                _currentPage = 1;
                _totalPages = 1;
                ErrorMessage = $"Error fetching products: {ex.Message}";
                _logger.LogError(ex, "Error fetching products");
            }
            finally
            {
                _isFetching = false;
                ListChanged?.Invoke();
                _logger.LogInformation("ListChanged invoked");
            }
        }
    }
}