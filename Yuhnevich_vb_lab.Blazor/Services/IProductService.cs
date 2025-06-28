namespace Yuhnevich_vb_lab.Blazor.Services
{
    public interface IProductService<T>
    {
        event Action ListChanged;
        IEnumerable<T> Products { get; }
        int CurrentPage { get; }
        int TotalPages { get; }
        Task GetProducts(int pageNo = 1, int pageSize = 3);
    }
}