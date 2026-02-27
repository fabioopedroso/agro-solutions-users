# AgroSolutions Kubernetes Manifests

Manifestos Kubernetes para deploy dos microsserviços AgroSolutions no Minikube.

## 📋 Estrutura

```
k8s/
├── namespace.yaml              # Namespace agrosolutions
├── configmap.yaml              # ConfigMap do Users API
├── deployment.yaml             # Deployment do Users API
├── service.yaml                # Services do Users API
├── deploy.sh                   # Script de deploy (Linux/Mac)
├── deploy.ps1                  # Script de deploy (Windows)
└── infra/
    ├── secrets.yaml            # Secrets (Supabase, JWT, RabbitMQ)
    ├── rabbitmq-deployment.yaml
    └── rabbitmq-service.yaml
```

## 🚀 Pré-requisitos

1. **Minikube** instalado e rodando
2. **kubectl** configurado
3. **Docker** para build das imagens
4. **Supabase** projeto criado com connection strings
5. **DockerHub** (ou registry) para armazenar imagens

## ⚙️ Configuração

### 1. Configurar Supabase

Edite `infra/secrets.yaml` e substitua as connection strings:

```yaml
supabase-users-connection: "postgresql://postgres:SEU_PASSWORD@db.SEU_PROJECT.supabase.co:5432/postgres?schema=users"
supabase-properties-connection: "postgresql://postgres:SEU_PASSWORD@db.SEU_PROJECT.supabase.co:5432/postgres?schema=properties"
supabase-analysis-connection: "postgresql://postgres:SEU_PASSWORD@db.SEU_PROJECT.supabase.co:5432/postgres?schema=analysis"
```

### 2. Gerar chave JWT

Edite `infra/secrets.yaml` e substitua:

```yaml
jwt-key: "sua-chave-secreta-super-segura-com-no-minimo-32-caracteres"
```

### 3. Build e Push das Imagens Docker

```bash
# Users
cd ../../../agro-solutions-users
docker build -t seu-dockerhub/agro-users:latest .
docker push seu-dockerhub/agro-users:latest

# Properties
cd ../agro-solutions-properties-fields
docker build -t seu-dockerhub/agro-properties:latest .
docker push seu-dockerhub/agro-properties:latest

# Sensor Ingestion
cd ../agro-solutions-sensor-ingestion
docker build -t seu-dockerhub/agro-sensor-ingestion:latest .
docker push seu-dockerhub/agro-sensor-ingestion:latest

# Analysis Alerts
cd ../agro-solutions-analysis-alerts
docker build -t seu-dockerhub/agro-analysis-alerts:latest .
docker push seu-dockerhub/agro-analysis-alerts:latest

# Sensor Simulator
cd ../agro-solutions-sensor-simulator
docker build -t seu-dockerhub/agro-sensor-simulator:latest .
docker push seu-dockerhub/agro-sensor-simulator:latest
```

### 4. Atualizar Deployments

Edite os arquivos `deployment.yaml` de cada serviço e atualize a imagem:

```yaml
image: seu-dockerhub/agro-users:latest
```

## 🎯 Deploy

### Linux/Mac

```bash
chmod +x deploy.sh
./deploy.sh
```

### Windows PowerShell

```powershell
.\deploy.ps1
```

### Deploy Manual

```bash
# Namespace e infraestrutura
kubectl apply -f namespace.yaml
kubectl apply -f infra/

# Aguardar RabbitMQ
kubectl wait --for=condition=ready pod -l app=rabbitmq -n agrosolutions --timeout=120s

# Microsserviços
kubectl apply -f .
kubectl apply -f ../../agro-solutions-properties-fields/k8s/
kubectl apply -f ../../agro-solutions-sensor-ingestion/k8s/
kubectl apply -f ../../agro-solutions-analysis-alerts/k8s/
kubectl apply -f ../../agro-solutions-sensor-simulator/k8s/
```

## 📊 Verificar Status

```bash
# Pods
kubectl get pods -n agrosolutions

# Services
kubectl get services -n agrosolutions

# Logs
kubectl logs -f deployment/users-api -n agrosolutions
kubectl logs -f deployment/analysis-alerts -n agrosolutions
```

## 🌐 Acessar Serviços

```bash
# Obter IP do Minikube
minikube ip

# Acessar serviços
# Users API
curl http://$(minikube ip):30001/swagger

# Properties API
curl http://$(minikube ip):30002/swagger

# Sensor Ingestion
curl http://$(minikube ip):30003/swagger

# RabbitMQ Management
# Browser: http://$(minikube ip):30672
# User: guest / Password: guest
```

## 🔄 Executar Migrations

Após o deploy, execute as migrations apontando para o Supabase:

```bash
# Users
cd AgroSolutions.Users
dotnet ef database update --connection "sua-connection-string-supabase-users"

# Properties
cd AgroSolutions.Properties.Fields
dotnet ef database update --connection "sua-connection-string-supabase-properties"

# Analysis
cd AgroSolutions.Analysis.Alerts
dotnet ef database update --connection "sua-connection-string-supabase-analysis"
```

## 👤 Criar Usuário Inicial

```bash
# Criar usuário via API
curl -X POST http://$(minikube ip):30001/api/users/register \
  -H "Content-Type: application/json" \
  -d '{
    "email": "admin@agrosolutions.com",
    "password": "Admin@123"
  }'

# Fazer login e obter token
curl -X POST http://$(minikube ip):30001/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "email": "admin@agrosolutions.com",
    "password": "Admin@123"
  }'
```

## 🔧 Configurar Simulator

Atualize o deployment do sensor-simulator com o token JWT:

```bash
kubectl edit deployment sensor-simulator -n agrosolutions

# Ou edite o arquivo ../../agro-solutions-sensor-simulator/k8s/deployment.yaml
# e atualize Authentication__Token com o token JWT obtido
```

## 🗑️ Remover Tudo

```bash
kubectl delete namespace agrosolutions
```

## 📝 Arquitetura

- **PostgreSQL**: Supabase (gerenciado externamente)
- **RabbitMQ**: Cluster Kubernetes (1 replica)
- **Users API**: 2 replicas, NodePort 30001
- **Properties API**: 2 replicas, NodePort 30002
- **Sensor Ingestion**: 2 replicas, NodePort 30003
- **Analysis Alerts**: 1 replica (worker, sem service)
- **Sensor Simulator**: 1 replica (worker, sem NodePort)

## 🔐 Segurança

- Todos os containers rodam como non-root (UID 1000)
- Secrets para credenciais sensíveis
- ConfigMaps para configurações não-sensíveis
- Resource limits definidos
- Health checks (liveness/readiness)

## 📈 Observabilidade

Os manifestos incluem:
- Liveness probes (container está vivo?)
- Readiness probes (container está pronto para receber tráfego?)
- Labels para identificação e filtering
- Resource requests e limits para métricas

---

Desenvolvido para o Hackathon 8NETT - AgroSolutions 🌱
