namespace CursoMongo.API.Domain.Enums;

public enum ECozinha
{
    Brasileira = 1,
    Italiana = 2,
    Arabe = 3,
    Japonesa = 4,
    FastFood = 5
}

public static class ECozinhaHelper
{
    public static ECozinha ConverteDeInteiroParaEnum(int valor)
    {
        var cozinha = (ECozinha)Enum.ToObject(typeof(ECozinha), valor);

        if (!Enum.IsDefined(typeof(ECozinha), cozinha))
        {
            throw new InvalidOperationException($"Não ha cozinha com o cod: {valor}!");
        }
        return cozinha;
    }
}
