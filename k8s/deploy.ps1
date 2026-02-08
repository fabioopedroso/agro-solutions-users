Write-Host "==========================================" -ForegroundColor Green
Write-Host "  Deploying AgroSolutions to Minikube" -ForegroundColor Green
Write-Host "==========================================" -ForegroundColor Green
Write-Host ""

# Get the current directory
$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path

Write-Host "📦 Step 1: Creating namespace..." -ForegroundColor Yellow
kubectl apply -f "$ScriptDir\namespace.yaml"
Write-Host ""

Write-Host "🔐 Step 2: Creating secrets..." -ForegroundColor Yellow
kubectl apply -f "$ScriptDir\infra\secrets.yaml"
Write-Host ""

Write-Host "🐰 Step 3: Deploying RabbitMQ..." -ForegroundColor Yellow
kubectl apply -f "$ScriptDir\infra\rabbitmq-deployment.yaml"
kubectl apply -f "$ScriptDir\infra\rabbitmq-service.yaml"
Write-Host ""

Write-Host "⏳ Waiting for RabbitMQ to be ready..." -ForegroundColor Yellow
kubectl wait --for=condition=ready pod -l app=rabbitmq -n agrosolutions --timeout=120s
Write-Host "✅ RabbitMQ is ready!" -ForegroundColor Green
Write-Host ""

Write-Host "🚀 Step 4: Deploying microservices..." -ForegroundColor Yellow

# Users API
Write-Host "  - Deploying Users API..." -ForegroundColor Cyan
kubectl apply -f "$ScriptDir\configmap.yaml"
kubectl apply -f "$ScriptDir\deployment.yaml"
kubectl apply -f "$ScriptDir\service.yaml"

# Properties API
Write-Host "  - Deploying Properties API..." -ForegroundColor Cyan
kubectl apply -f "$ScriptDir\..\..\agro-solutions-properties-fields\k8s\"

# Sensor Ingestion
Write-Host "  - Deploying Sensor Ingestion..." -ForegroundColor Cyan
kubectl apply -f "$ScriptDir\..\..\agro-solutions-sensor-ingestion\k8s\"

# Analysis Alerts Worker
Write-Host "  - Deploying Analysis Alerts Worker..." -ForegroundColor Cyan
kubectl apply -f "$ScriptDir\..\..\agro-solutions-analysis-alerts\k8s\"

# Sensor Simulator
Write-Host "  - Deploying Sensor Simulator..." -ForegroundColor Cyan
kubectl apply -f "$ScriptDir\..\..\agro-solutions-sensor-simulator\k8s\"

Write-Host ""
Write-Host "✅ Deployment complete!" -ForegroundColor Green
Write-Host ""
Write-Host "==========================================" -ForegroundColor Green
Write-Host "  Cluster Status" -ForegroundColor Green
Write-Host "==========================================" -ForegroundColor Green
kubectl get pods -n agrosolutions
Write-Host ""
Write-Host "==========================================" -ForegroundColor Green
Write-Host "  Service Endpoints" -ForegroundColor Green
Write-Host "==========================================" -ForegroundColor Green
try {
    $MinikubeIP = minikube ip 2>$null
} catch {
    $MinikubeIP = "localhost"
}
Write-Host "PostgreSQL: Supabase (gerenciado externamente)" -ForegroundColor White
Write-Host "RabbitMQ Management: http://$MinikubeIP:30672" -ForegroundColor White
Write-Host "Users API: http://$MinikubeIP:30001" -ForegroundColor White
Write-Host "Properties API: http://$MinikubeIP:30002" -ForegroundColor White
Write-Host "Sensor Ingestion: http://$MinikubeIP:30003" -ForegroundColor White
Write-Host ""
Write-Host "==========================================" -ForegroundColor Green
Write-Host "  Next Steps" -ForegroundColor Green
Write-Host "==========================================" -ForegroundColor Green
Write-Host "1. Configure Supabase connection strings in infra\secrets.yaml" -ForegroundColor Yellow
Write-Host "2. Build and push Docker images to registry" -ForegroundColor Yellow
Write-Host "3. Update deployment.yaml files with correct image names" -ForegroundColor Yellow
Write-Host "4. Run migrations to Supabase" -ForegroundColor Yellow
Write-Host "5. Create a user and get JWT token" -ForegroundColor Yellow
Write-Host "6. Update sensor-simulator deployment with JWT token" -ForegroundColor Yellow
Write-Host ""
