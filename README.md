# OWIN / Web API 2 / OAuth

Minimalbeispiel für die Authentifizierung mittels OAuth bei Benutzung der Web API 2 in einer Self-Hosted-Applikation mittels OWIN.

## Bauen unter Mono

Folgende Docker-Kommandos ausführen

	docker build -t owin-test . 
	docker run --rm owin-test

wobei `owin-test` ein beliebiger Name für das neue Image ist.
Als Einzeiler:

	docker build -t owin-test . && docker run --rm owin-test
