FROM python:3.10.1-slim-bullseye

WORKDIR /app

# Install dependancies, cached
COPY requirements.txt ./
RUN pip install --no-cache-dir -r requirements.txt

# Copy over everything else
COPY . .

# Check markdown and yaml style
RUN mdformat --check .
RUN yamllint -c .yamllint.yaml .