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

## Como Funciona

Ao ser executada, a aplicação inicia um loop de monitoramento contínuo que busca o preço do ativo a cada segundo. O console exibe a cotação atual e um gráfico para facilitar a visualização.

### Visualização Gráfica

Um gráfico (gerado usando uma biblioteca de gráficos) é renderizado diretamente no console para mostrar a flutuação do preço em relação aos seus limites.

- **Escala Dinâmica:** O eixo Y (preço) do gráfico não começa do zero. Ele é ajustado dinamicamente para focar no range entre o seu preço de compra e o seu preço de venda, oferecendo uma visualização muito mais clara da ação do preço em torno dos seus limites.

- **Cores dos Limites:**

  - **Linha de Venda (Azul):** Uma linha no topo do gráfico mostrando seu limite de venda.

  - **Linha de Compra (Vermelha):** Uma linha na base do gráfico mostrando seu limite de compra

  - **Preço Atual (Verde):** A linha que se move, mostrando a cotação atual do ativo.

### Lógica de Alertas (Anti-Spam)

A aplicação possui um motor de lógica com estado para enviar alertas de forma inteligente e evitar o envio excessivo de e-mails (spam)

- **Envio Único:** Quando o preço cruza um limite (por exemplo, cai abaixo do preço de compra), um e-mail de alerta é enviado apenas uma vez.

- **Reset de Estado:** Para que um novo alerta de compra seja enviado, o preço precisa primeiro subir de volta acima do limite de compra (resetando o "estado" do alerta) e, em seguida, cair abaixo dele novamente.

- **Exemplo Prático:** Se o preço de compra é R$ 25,00 e a cotação cai para R$ 24,90, você recebe um e-mail. Se o preço continuar caindo para R$ 24,80, você não receberá outro e-mail. No entanto, se o preço subir para R$ 25,10 e depois cair para R$ 24,95, você receberá um segundo e-mail, pois o alerta foi "rearmado". A mesma lógica se aplica ao limite de venda.


## Como Rodar os Testes

O projeto inclui uma suíte de testes unitários e de integração. Para rodar os testes, navegue até a pasta `StockMonitor.Tests` e execute o seguinte comando:

```bash
cd ..\StockMonitor.Tests
dotnet test --logger "console;verbosity=detailed" 
```
