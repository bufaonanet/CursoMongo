using FluentValidation;
using FluentValidation.Results;

namespace CursoMongo.API.Domain.ValueObjects;

public class Endereco : AbstractValidator<Endereco>
{
    public Endereco(string logradouro, string numero, string cidade, string uf, string cep)
    {
        Logradouro = logradouro;
        Numero = numero;
        Cidade = cidade;
        UF = uf;
        Cep = cep;
    }

    public string Logradouro { get; init; }
    public string Numero { get; init; }
    public string Cidade { get; init; }
    public string UF { get; init; }
    public string Cep { get; init; }

    public ValidationResult ValidationResult { get; set; }

    public bool Validar()
    {
        ValidarLogradouro();
        ValidarCidade();
        ValidarCep();
        ValidarUF();

        ValidationResult = Validate(this);

        return ValidationResult.IsValid;
    }

    private void ValidarLogradouro()
    {
        RuleFor(c => c.Logradouro)
            .NotEmpty().WithMessage("Logradouro não pode ser vazio.")
            .MaximumLength(50).WithMessage("Logradouro pode ter no maximo 50 caracteres.");
    }

    private void ValidarCidade()
    {
        RuleFor(c => c.Cidade)
            .NotEmpty().WithMessage("Cidade não pode ser vazio.")
            .MaximumLength(100).WithMessage("Cidade pode ter no maximo 100 caracteres.");
    }

    private void ValidarUF()
    {
        RuleFor(c => c.UF)
            .NotEmpty().WithMessage("UF não pode ser vazio.")
            .Length(2).WithMessage("UF deve ter 2 caracteres.");
    }

    private void ValidarCep()
    {
        RuleFor(c => c.Cep)
            .NotEmpty().WithMessage("Cep não pode ser vazio.")
            .Length(8).WithMessage("Cep deve ter 8 caracteres.");
    }

}
