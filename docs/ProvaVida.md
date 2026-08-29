## ProvaVida - Prova de Vida

### Descrição 

Aplicativo Movel para que os usuários possam provar que estão vivos atravéz de check in diário.
Caso o usuario deixe de fazer check in no dia ate as 20h deve ser emitido uma notificação para o usuario no celular
Caso o usuario deixe de fazer check in por 2 dias seguidos ate as 21h do segundo dia deve ser emitido uma notificação para o usuario no celular
Caso o usuário deixe de fazer check in por 2 dias seguidos no no terceiro dia o App vai enviar mensagem para o contato de emergência do usuário.

### Funcionalidades

* Cadastrar Dados da Conta
* Alterar Dados da Conta
* Remover Dados da Conta
* Efetuar Login no App
* Efetuar Logoff no App
* Fazer Check in no App
* Sincronizar dados entre servidor e celular a cada 1h (quando o celular tenha internet)
* Enviar Heartbeat a cada 3h (quando o celular tenha internet)
* Enviar Mensagem para o Proprio Usuário 
* Enviar Mensagem para Contato de Emergência 
* Painel Admistrativo

#### Cadastrar Dados da Conta 

Permitir que o usuário possa se cadastrar no App, o cadastro vai ter dados básicos: **nome, e-mail, whatsapp, senha, contato de emergência (nome, e-mail, whatsapp)** 

#### Alterar Dados da Conta 

Permitir que o usuário altere os dados da sua conta inclusive senha no App

#### Remover Dados da Conta 

Permitir que o usuário removs seus dados do App

#### Efetuar Login no App

Permitir que o usuário inicie uma sessão de uso no App

#### Efetuar Logoff no App

Permitir que o usuário finalize sessão de uso App

#### Efetuar Check in no App

Permitir que o usuário faça o check in diário para provar que esta vivo, quando o usuário realizar o check in vão ser armazenados os seguintes dados: **id do usuário, data, localização, identificação do aparelho, sincronizado**
Não permitir mais de um check in por dia

#### Sincronizar dados entre servidor e celular a cada 1h (quando o celular tenha internet)

Sincronizar os dados do servidor com celular e dados do celular com servidor, os dados que devem ser sincronizados são os dados de checkin e dados de conta, no sincronismo se os dados desse usuario teiverem sido excluidos do servidor os dados devem ser apagaod do aparelho e voltar para tela de login caso o App esteja aberto

#### Enviar Heartbeat a cada 3h (quando o celular tenha internet)

Enviar um *sinal de vida* para o servidor mostrando que o celular esta online

#### Enviar Mensagem para o Proprio Usuário 

Verificar se o usuario esta já um dia sem efetuar check in então notificar o usuario por email, whatsapp, sms e se não funcionar nenhuma das outras por ligação de voz

#### Enviar Mensagem para Contato de Emergência 

Verificar dentre os usuários se existem algum usuário que deixou de realizar check in diário por dois dias seguintes então notificar o contato de emergência dele para por email, whatsapp, sms e se não funcionar nenhuma das outras por ligação de voz

#### Painel Adminstrativo

Um painel para que possa administrar os dados do servidor , com informações de usuario cadastrados, usuario com checkin atrasado, notificações enviadas aos usuários, notificações enviadas aos contatos de emergeicias, checkins efetuados, notificações enviadas, testes de envio de notificações email, whatsapp, sms e ligação de voz e controle de backup de banco do servidor
