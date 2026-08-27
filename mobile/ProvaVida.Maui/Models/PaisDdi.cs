namespace ProvaVida.Maui.Models;

/// <summary>
/// Representa um país com seu código DDI para uso no seletor de telefone.
/// </summary>
public sealed record PaisDdi(string Bandeira, string Nome, string Codigo)
{
    /// <summary>Texto exibido no Picker — ex: "🇧🇷 +55"</summary>
    public string Exibicao => $"{Bandeira} +{Codigo}";

    public static IReadOnlyList<PaisDdi> Todos { get; } =
    [
        new("🇧🇷", "Brasil",         "55"),
        new("🇺🇸", "EUA",            "1"),
        new("🇵🇹", "Portugal",       "351"),
        new("🇦🇷", "Argentina",      "54"),
        new("🇨🇱", "Chile",          "56"),
        new("🇨🇴", "Colômbia",       "57"),
        new("🇲🇽", "México",         "52"),
        new("🇺🇾", "Uruguai",        "598"),
        new("🇵🇾", "Paraguai",       "595"),
        new("🇧🇴", "Bolívia",        "591"),
        new("🇬🇧", "Reino Unido",    "44"),
        new("🇩🇪", "Alemanha",       "49"),
        new("🇫🇷", "França",         "33"),
        new("🇪🇸", "Espanha",        "34"),
        new("🇮🇹", "Itália",         "39"),
        new("🇯🇵", "Japão",          "81"),
        new("🇨🇳", "China",          "86"),
        new("🇮🇳", "Índia",          "91"),
        new("🇦🇺", "Austrália",      "61"),
        new("🇨🇦", "Canadá",         "1"),
    ];

    public static PaisDdi Padrao => Todos[0]; // Brasil
}
