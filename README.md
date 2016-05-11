# RecordLinkagePipelineDemo
Exploring linking records from disparate data sources

![Screenshot](/Pipeline.png)

### Building and running the pipeline with Mono on an Ubuntu Vagrant instance
1. [Local] Start the vagrant instance with `vagrant up`
2. [Local] Login to the vagrant instance with `vagrant ssh`
3. [Instance] Build the solution with mono and run the pipeline with `/vagrant/vagrant/build_and_run.bash`
4. [Either] **result.txt** and **log.txt** should have been generated in the vagrant shared directory.

### TODO
* Instructions: building on windows
* Make RecordLinkagePipeline a git sub-module instead of hiding it inside the vagrant shared folder.
