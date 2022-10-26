test:
	docker build -f Dockerfile.SystemTests -t systemtests .
	docker run systemtests