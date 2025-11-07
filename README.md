# StockMonitor

Este é um aplicativo de console .NET que monitora o preço de uma ação e envia alertas por e-mail quando o preço atinge determinados limites de compra ou venda.

## Pré-requisitos

- [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [Git](https://git-scm.com/downloads)

## Como Baixar

Clone o repositório para sua máquina local:

```bash
git clone <URL_DO_REPOSITORIO>
cd Project Inoa
```

## Como Configurar

O aplicativo requer algumas configurações para funcionar corretamente, principalmente para o envio de e-mails.

### 1. Configurações de SMTP

As configurações do servidor de e-mail (SMTP) são carregadas a partir do arquivo `StockMonitor/appsettings.json`. Abra este arquivo e edite a seção `SmtpSettings` com as informações do seu provedor de e-mail:

```json
{
  "SmtpSettings": {
    "EmailDestino": "seu-email-de-destino@exemplo.com",
    "SmtpServidor": "smtp.seuprovedor.com",
    "SmtpPorta": 587,
    "SmtpUsuario": "seu-usuario-smtp@exemplo.com"
  }
}
```

### 2. Senha do SMTP

Por razões de segurança, a senha do SMTP não é armazenada no `appsettings.json`. Em vez disso, ela é carregada a partir de um arquivo `.env`.

Crie um arquivo chamado `.env` dentro da pasta projeto (`Project Inoa/.env`) e adicione a seguinte linha:

```
SmtpSettings__SmtpSenha=sua-senha-smtp-aqui (sem aspas)
```

**Importante:** O arquivo `.env` está no `.gitignore` e não deve ser enviado para o repositório.

## Como Rodar

O aplicativo é executado a partir da linha de comando, e você precisa fornecer três argumentos:

1.  **Ativo:** O ticker da ação que você quer monitorar (ex: `PETR4` para Petrobras).
2.  **Preço de Venda:** O preço acima do qual um alerta de venda deve ser enviado.
3.  **Preço de Compra:** O preço abaixo do qual um alerta de compra deve ser enviado.

Para rodar o aplicativo, navegue até a pasta `StockMonitor` e use o comando `dotnet run`:

```bash
cd StockMonitor
dotnet run -- PETR4 30.50 25.00
```

Neste exemplo, o aplicativo irá monitorar a ação `PETR4.SA`, enviando um alerta se o preço subir para R$ 30,50 ou mais, ou se cair para R$ 25,00 ou menos.

## Como Rodar os Testes

O projeto inclui uma suíte de testes unitários e de integração. Para rodar os testes, navegue até a pasta `StockMonitor.Tests` e execute o seguinte comando:

```bash
cd ..\StockMonitor.Tests
dotnet test
```
