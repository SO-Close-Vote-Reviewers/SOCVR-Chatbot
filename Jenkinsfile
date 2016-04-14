//main build logic

node('swarm') {
	stage 'checkout'
	checkout scm
	
	stage 'build docker image'
	dockerBuild()
}

def dockerBuild() {
    
    def imageName = 'socvr/chatbot'
	def tagString = ":latest"
    
    //for this sort of build, it doesn't matter what the tag name is

	def shCommand = "docker build -t ${imageName}${tagString} ."
	sh shCommand
}
