{{- define "commercehub.name" -}}
{{- default .Chart.Name .Values.nameOverride | trunc 63 | trimSuffix "-" }}
{{- end }}

{{- define "commercehub.fullname" -}}
{{- if .Values.fullnameOverride }}
{{- .Values.fullnameOverride | trunc 63 | trimSuffix "-" }}
{{- else }}
{{- $name := default .Chart.Name .Values.nameOverride }}
{{- if contains $name .Release.Name }}
{{- .Release.Name | trunc 63 | trimSuffix "-" }}
{{- else }}
{{- printf "%s-%s" .Release.Name $name | trunc 63 | trimSuffix "-" }}
{{- end }}
{{- end }}
{{- end }}

{{- define "commercehub.labels" -}}
helm.sh/chart: {{ include "commercehub.name" . }}-{{ .Chart.Version | replace "+" "_" }}
{{ include "commercehub.selectorLabels" . }}
{{- if .Chart.AppVersion }}
app.kubernetes.io/version: {{ .Chart.AppVersion | quote }}
{{- end }}
app.kubernetes.io/managed-by: {{ .Release.Service }}
{{- end }}

{{- define "commercehub.selectorLabels" -}}
app.kubernetes.io/name: {{ include "commercehub.name" . }}
app.kubernetes.io/instance: {{ .Release.Name }}
{{- end }}

{{- define "commercehub.probes" -}}
livenessProbe:
  httpGet:
    path: /health/live
    port: http
  initialDelaySeconds: 30
  periodSeconds: 15
  timeoutSeconds: 5
  failureThreshold: 3
readinessProbe:
  httpGet:
    path: /health/ready
    port: http
  initialDelaySeconds: 15
  periodSeconds: 10
  timeoutSeconds: 3
  failureThreshold: 2
startupProbe:
  httpGet:
    path: /health
    port: http
  initialDelaySeconds: 5
  periodSeconds: 5
  timeoutSeconds: 3
  failureThreshold: 30
{{- end }}

{{- define "commercehub.resources" -}}
resources:
  requests:
    memory: {{ .Values.resources.requests.memory | default "256Mi" }}
    cpu: {{ .Values.resources.requests.cpu | default "250m" }}
  limits:
    memory: {{ .Values.resources.limits.memory | default "512Mi" }}
    cpu: {{ .Values.resources.limits.cpu | default "500m" }}
{{- end }}
