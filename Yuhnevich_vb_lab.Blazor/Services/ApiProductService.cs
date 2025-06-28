using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
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
        private int _lastPageNo = 0;
        private int _lastPageSize = 0;
        private bool _isFetching = false;

public IEnumerable<Dish> Products => _dishes;
        public int CurrentPage => _currentPage;
        public int TotalPages => _totalPages;
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

            if (pageNo == _lastPageNo && pageSize == _lastPageSize && _dishes.Any())
            {
                _logger.LogInformation("Skipping duplicate request for pageNo={PageNo}, pageSize={PageSize}", pageNo, pageSize);
                return;
            }

            _isFetching = true;
            try
            {
                var uri = new Uri(_httpClient.BaseAddress, "api/dishes");
                var queryData = new Dictionary<string, string>
{
{ "pageNo", pageNo.ToString() },
{ "pageSize", pageSize.ToString() }
};
                var query = QueryString.Create(queryData);
                var fullUri = uri + query.Value;
                _logger.LogInformation("Sending GET request to {Uri}", fullUri);
                var result = await _httpClient.GetAsync(fullUri);

                if (result.IsSuccessStatusCode)
                {
                    var responseData = await result.Content.ReadFromJsonAsync<ResponseData<ListModel<Dish>>>();
                    _logger.LogInformation("API response: Success={Success}, ItemsCount={Count}, Error={Error}",
                    responseData?.Success, responseData?.Data?.Items?.Count, responseData?.ErrorMessage);
                    if (responseData?.Success == true)
                    {
                        _currentPage = responseData.Data.CurrentPage;
                        _totalPages = responseData.Data.TotalPages;
                        _dishes = responseData.Data.Items ?? new List<Dish>();
                        _lastPageNo = pageNo;
                        _lastPageSize = pageSize;
                        _logger.LogInformation("Products loaded: {Products}", string.Join(", ", _dishes.Select(d => d.Name)));
                        ListChanged?.Invoke();
                        _logger.LogInformation("ListChanged invoked");
                    }
                    else
                    {
                        _dishes = new List<Dish>();
                        _currentPage = 1;
                        _totalPages = 1;
                        _lastPageNo = 0;
                        _lastPageSize = 0;
                        _logger.LogWarning("API returned unsuccessful response: {Error}", responseData?.ErrorMessage);
                        ListChanged?.Invoke();
                    }
                }
                else
                {
                    _dishes = new List<Dish>();
                    _currentPage = 1;
                    _totalPages = 1;
                    _lastPageNo = 0;
                    _lastPageSize = 0;
                    _logger.LogError("API request failed with status code {StatusCode}", result.StatusCode);
                    ListChanged?.Invoke();
                }
            }
            catch (Exception ex)
            {
                _dishes = new List<Dish>();
                _currentPage = 1;
                _totalPages = 1;
                _lastPageNo = 0;
                _lastPageSize = 0;
                _logger.LogError(ex, "Error fetching products");
                ListChanged?.Invoke();
            }
            finally
            {
                _isFetching = false;
            }
        }
    }
}