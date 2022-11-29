namespace CursoMongo.API.Controllers.Outputs;

internal class RestauranteExibicao
{
    public string Id { get; set; }
    public string Nome { get; set; }
    public int Cozinha { get; set; }
    public EnderecoExibicao Endereco { get; set; }
}