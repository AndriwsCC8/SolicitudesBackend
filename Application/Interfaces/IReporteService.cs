namespace Application.Interfaces
{
    public interface IReporteService
    {
        Task<byte[]> GenerarReporteExcelAsync();
    }
}
