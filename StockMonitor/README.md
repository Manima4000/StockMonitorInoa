# StockMonitor

## Sobre o Projeto
O **StockMonitor** é um programa simples desenvolvido em **C#** para monitorar o preço de um ativo na bolsa de valores.  
O programa consulta as cotações de um ativo usando a API do **Yahoo Finance** e envia **alertas por e-mail** caso o preço atinja valores pré-definidos para venda ou compra.

## Configuração Inicial

### 1. Edite o Arquivo `config.txt`
O arquivo `config.txt` é usado para armazenar informações essenciais para o envio de e-mails.  
Você precisa ajustá-lo para incluir:

- O **e-mail** que receberá os alertas.
- O **e-mail** que enviará os alertas.
- As **configurações do servidor SMTP**.

Aqui está o formato do arquivo `config.txt`:

```
<email-que-recebera-alertas>
<smtp-servidor>
<smtp-porta>
<email-que-enviara-alertas>
```

**Exemplo:**
```
destinatario@email.com
smtp.gmail.com
587
meuemail@gmail.com
```

> **Nota:** A senha do e-mail **não** é incluída no arquivo `config.txt`.  
> Ela é armazenada de forma segura utilizando o **dotnet user-secrets**.  
> Veja os detalhes abaixo.

### 2. Configure a Senha nos User Secrets
Por motivos de **segurança**, a senha do e-mail usado para enviar os alertas é armazenada nos **User Secrets**.  
Para configurar a senha, execute os seguintes comandos no terminal dentro do diretório do projeto:

```bash
dotnet user-secrets init
dotnet user-secrets set "SMTP_PASSWORD" "sua_senha_aqui"
```

Para confirmar que a senha foi salva corretamente, use:

```bash
dotnet user-secrets list
```

> **Nota:** Certifique-se de que a senha configurada pertence ao e-mail configurado para enviar os alertas.

## Como o Código Funciona

### 1. Entrada de Dados
Ao rodar o programa, você precisa informar três parâmetros na **linha de comando**:

1. O **código do ativo** a ser monitorado (ex.: `PETR4.SA`).
2. O **preço-limite de venda**.
3. O **preço-limite de compra**.

**Exemplo de execução:**
```bash
dotnet run -- PETR4 35.70 35.40
```

### 2. Monitoramento da Cotação
O programa:

- Consulta os preços atuais do ativo usando a **API Yahoo Finance**.
- Exibe as cotações no terminal em intervalos regulares (**60 segundos** por padrão).
- Verifica se o preço alcançou os **limites de compra ou venda** informados.

### 3. Envio de Alertas por E-mail
Caso o preço atinja o **limite de compra ou venda**, o programa envia um **e-mail de alerta** para o destinatário configurado.

Para o envio do e-mail, o programa utiliza as configurações SMTP informadas no `config.txt` e a senha armazenada em **dotnet user-secrets**.

### 4. Estrutura do Código
O código principal está organizado da seguinte forma:

- **Leitura do `config.txt`**: Coleta as configurações do e-mail e do servidor SMTP.
- **Consulta da API Yahoo Finance**: Obtém os preços do ativo especificado.
- **Envio do E-mail**: Utiliza o `SmtpClient` para enviar o alerta.
- **Loop de Monitoramento**: Atualiza as cotações a cada minuto e verifica os preços.

## Dependências
O projeto utiliza os seguintes pacotes **NuGet**:

- **YahooFinanceApi**: Para obter os dados financeiros.
- **Microsoft.Extensions.Configuration**: Para trabalhar com segredos e configurações.
