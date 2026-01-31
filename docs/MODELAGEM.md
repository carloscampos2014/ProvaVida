# Modelagem de Dados - Prova de Vida

Este documento descreve as entidades do sistema e suas propriedades. Todas as entidades devem ser implementadas no projeto `ProvaDeVida.Dominio`.

## 1. Entidade: Usuario
Representa a pessoa que está sendo monitorada.
- **Id**: Guid (Chave Primária)
- **Nome**: String (Obrigatório)
- **Email**: String (Obrigatório, deve ser único)
- **Telefone**: String (Obrigatório)
- **SenhaHash**: String (Para autenticação)
- **DataUltimoCheckIn**: DateTime (Atualizado a cada novo check-in)
- **DataProximoCheckIn**: DateTime (Calculado: DataUltimoCheckIn + 48 horas)
- **Status**: Enum (Ativo, EmAtraso, AlertaCritico)

## 2. Entidade: ContatoEmergencia
Pessoas que serão notificadas em caso de ausência do usuário.
- **Id**: Guid
- **UsuarioId**: Guid (Chave Estrangeira)
- **Nome**: String (Obrigatório)
- **Email**: String (Obrigatório)
- **TelefoneWhatsapp**: String (Obrigatório)
- **Prioridade**: Int (Ordem de disparo, caso haja mais de um)

## 3. Entidade: RegistroCheckIn (Histórico Reduzido)
Armazena as confirmações de vida. 
- **Regra:** Manter apenas os últimos 5 registros por usuário.
- **Id**: Guid
- **UsuarioId**: Guid
- **DataHora**: DateTime
- **Localizacao**: String (Opcional - Coordenadas GPS se disponível)

## 4. Entidade: HistoricoNotificacao
Registra os alertas enviados aos contatos.
- **Regra:** Manter apenas os últimos 5 registros de envio.
- **Id**: Guid
- **UsuarioId**: Guid
- **ContatoId**: Guid
- **DataEnvio**: DateTime
- **Tipo**: Enum (Lembrete, AlertaEmergencia, AlertaCritico)
- **Sucesso**: Boolean