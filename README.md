# StockMonitor

[![CI/CD Workflow](https://github.com/Manima4000/StockMonitorInoa/actions/workflows/ci-cd.yml/badge.svg)](https://github.com/Manima4000/StockMonitorInoa/actions/workflows/ci-cd.yml)

Este é um aplicativo de console .NET que monitora o preço de múltiplas ações e envia alertas por e-mail quando os preços atingem determinados limites de compra ou venda.

## Pré-requisitos

- [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [Git](https://git-scm.com/downloads)

## Como Baixar

Clone o repositório para sua máquina local:

```bash
git clone https://github.com/Manima4000/StockMonitorInoa
cd StockMonitorInoa
```

## Como Configurar

O aplicativo requer algumas configurações para funcionar corretamente.

### 1. Configuração das Ações

Abra o arquivo `StockMonitorInoa/StockMonitor/appsettings.json`. A configuração dos ativos a serem monitorados é feita na seção `"Stocks"`. Você pode adicionar quantos ativos quiser a esta lista.

```json
{
  "Stocks": [
    {
      "Ticker": "PETR4",
      "SellPrice": 30.0,
      "BuyPrice": 25.0,
      "SmaPeriod": 5
    },
    {
      "Ticker": "VALE3",
      "SellPrice": 70.0,
      "BuyPrice": 65.0,
      "SmaPeriod": 10
    },
    {
      "Ticker": "MGLU3",
      "SellPrice": 2.50,
      "BuyPrice": 1.50,
      "SmaPeriod": 8
    }
  ]
}
```

### 2. Configurações de SMTP

As configurações do servidor de e-mail (SMTP) também ficam no `StockMonitorInoa/StockMonitor/appsettings.json`. Edite a seção `SmtpSettings` com as informações do seu provedor de e-mail:

```json
{
  "SmtpSettings": {
    "EmailDestino": "seu-email-de-destino@exemplo.com",
    "SmtpServidor": "smtp.seuprovedor.com",
    "SmtpPorta": 587,
    "SmtpUsuario": "seu-usuario-smtp@exemplo.com",
    "SmtpSenha": "" // Deixe em branco aqui
  }
}
```

### 3. Senha do SMTP

Por razões de segurança, a senha do SMTP não é armazenada no `appsettings.json`. Em vez disso, ela é carregada a partir de um arquivo `.env`.

Crie um arquivo chamado `.env` dentro da pasta `StockMonitor` (`StockMonitorInoa/StockMonitor/.env`) e adicione a seguinte linha:

```
SmtpSettings__SmtpSenha=sua-senha-smtp-aqui
```

**Importante:** O arquivo `.env` está no `.gitignore` e não deve ser enviado para o repositório.

## Como Rodar Localmente

Para rodar o aplicativo, navegue até a pasta `StockMonitor` e use o comando `dotnet run`:

```bash
cd StockMonitor
dotnet run
```

O aplicativo irá carregar a configuração do `appsettings.json` e começar a monitorar os ativos listados.

## Como Rodar com Docker

Você pode rodar o aplicativo usando Docker. Certifique-se de ter o Docker instalado.

### 1. Construir a Imagem Docker

Navegue até o diretório raiz do projeto (`StockMonitorInoa`) e construa a imagem Docker:

```bash
docker build -t stockmonitor .
```

### 2. Executar o Contêiner Docker

Para executar o contêiner, você precisará do arquivo `.env` na pasta `StockMonitor` (conforme descrito na seção "3. Senha do SMTP"). O comando de execução é:

```bash
docker run --rm -it --env-file StockMonitor/.env stockmonitor
```

O contêiner irá iniciar e usar a configuração de ativos definida no `appsettings.json` da imagem.

## Como Funciona

Ao ser executada, a aplicação inicia um loop que busca os preços de todos os ativos configurados a cada 2 segundos. O console exibe uma dashboard em tempo real com os dados de todos os ativos.

### Visualização no Console

Uma tabela (gerada com a biblioteca Spectre.Console) é renderizada diretamente no console, mostrando:
- **Ativo:** O ticker da ação.
- **Preço Atual:** A cotação atual. O preço fica verde se estiver acima do alvo de venda e vermelho se estiver abaixo do alvo de compra.
- **MMS:** A Média Móvel Simples calculada para o período configurado.
- **Alvo Compra/Venda:** Os limites que você configurou.
- **Variação:** Uma seta ▲ (verde) ou ▼ (vermelha) indicando a última variação de preço.

### Lógica de Alertas (Anti-Spam)

A aplicação possui um motor de lógica para cada ativo que evita o envio excessivo de e-mails (spam) utilizando um período de cooldown.

- **Cooldown de Alertas:** Quando o preço de um ativo cruza um limite (por exemplo, cai abaixo do preço de compra ou sobe acima do preço de venda), um e-mail de alerta é enviado. Após o envio, um período de cooldown de 5 minutos é ativado para aquele limite específico.
- **Reenvio Após Cooldown:** Novos alertas para o mesmo limite (compra ou venda) só serão enviados se o preço ainda estiver fora da faixa e o período de cooldown tiver expirado. Isso significa que, se o preço permanecer abaixo do limite de compra (ou acima do limite de venda) por mais de 5 minutos, um novo alerta será enviado.
- **Exemplo Prático:** Se o preço de compra para PETR4 é R$ 25,00 e a cotação cai para R$ 24,90, você recebe um e-mail. Se o preço continuar caindo para R$ 24,80 dentro dos 5 minutos de cooldown, você não receberá outro e-mail. No entanto, se o preço permanecer em R$ 24,80 e 5 minutos se passarem desde o último alerta, um novo e-mail será enviado. A mesma lógica se aplica ao limite de venda e funciona de forma independente para cada ativo.

## Como Rodar os Testes

O projeto inclui uma suíte de testes unitários e de integração. Para rodar os testes, navegue até a pasta raiz do projeto e execute:

```bash
dotnet test StockMonitorSolution.sln
```

## Estrutura do Projeto

O projeto é construído em torno de uma arquitetura de serviços desacoplada, facilitando a manutenção e os testes.

### Worker

-   **`StockMonitorWorker.cs`**: É o coração da aplicação. Como um `BackgroundService`, ele orquestra todo o fluxo de trabalho em um loop contínuo:
    -   Busca os preços atuais das ações usando o `IPriceProvider`.
    -   Calcula indicadores técnicos (como a Média Móvel Simples) através do `ITechnicalAnalysisService`.
    -   Verifica se algum alerta de compra ou venda deve ser disparado com o `IAlertingEngine`.
    -   Envia notificações por e-mail usando o `INotificationService`.
    -   Renderiza a tabela de dados no console através do `IChartService`.

### Serviços

-   **`YahooPriceProvider.cs`**: Implementação do `IPriceProvider` que busca os preços das ações na API do Yahoo Finance. Inclui uma política de resiliência (retry) para lidar com falhas de rede.
-   **`TechnicalAnalysisService.cs`**: Contém a lógica para cálculos de análise técnica, como a Média Móvel Simples (SMA).
-   **`AlertingEngine.cs`**: Motor de decisão que, para cada ativo, verifica se o preço atual cruzou os umbrais de compra ou venda definidos. Possui uma lógica de *cooldown* para evitar o envio de múltiplos alertas em um curto espaço de tempo.
-   **`EmailNotificationService.cs`**: Implementação do `INotificationService` que envia os alertas por e-mail usando as configurações de SMTP.
-   **`SpectreChartService.cs`**: Responsável por renderizar a tabela de monitoramento no console de forma organizada e visualmente agradável, utilizando a biblioteca Spectre.Console.
-   **`DateTimeProvider.cs`**: Abstrai o `DateTime.Now`, permitindo a injeção de um provedor de data e hora. Isso é crucial para tornar o código testável, especialmente a lógica de *cooldown* que depende do tempo.

### Interfaces

As interfaces definem os contratos para os serviços, garantindo o baixo acoplamento e a alta coesão:

-   **`IPriceProvider.cs`**: Contrato para serviços que fornecem preços de ativos.
-   **`ITechnicalAnalysisService.cs`**: Contrato para serviços de análise técnica.
-   **`IAlertingEngine.cs`**: Contrato para o motor de alertas.
-   **`INotificationService.cs`**: Contrato para serviços de notificação.
-   **`IChartService.cs`**: Contrato para serviços de exibição de dados.
-   **`IDateTimeProvider.cs`**: Contrato para provedores de data e hora.

### Settings

As classes de configuração são usadas para carregar dados do `appsettings.json` e de variáveis de ambiente de forma fortemente tipada.

-   **`MonitorSettings.cs`**: Modela as configurações de um único ativo a ser monitorado (ticker, preço de compra/venda, período da SMA).
-   **`SmtpSettings.cs`**: Modela as configurações do servidor SMTP para o envio de e-mails.

