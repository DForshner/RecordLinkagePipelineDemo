# RecordLinkagePipelineDemo
Exploring linking records from disparate data sources

![Screenshot](/Pipeline.png)

This demo is intended to run on Mono inside a [Vagrant](https://www.vagrantup.com/) instance so you will need Vagrant installed. 

### Provisioning the vagrant instance, building, and running the pipeline from scratch
1. [Local] Start the vagrant instance with `vagrant up`
2. **result.txt** and **log.txt** should have been generated in the vagrant/RecordLinkagePipeline shared directory.

### Building and running the pipeline after the instance is provisioned
1. [Local] Login to the vagrant instance with `vagrant ssh`
2. [Instance] Build the solution with mono and run the pipeline with `/vagrant/vagrant/build_and_run.bash`
3. **result.txt** and **log.txt** should have been generated in the vagrant/RecordLinkagePipeline shared directory.

### [Alternative] Opening in visual studio
The solution file can be found at `vagrant/RecordLinkagePipeline/RecordLinkagePipeline.sln`.  The only dependency should be Json.Net, which Nuget should take care of.

### TODO
* Make RecordLinkagePipeline a git sub-module instead of hiding it inside the vagrant shared folder.
