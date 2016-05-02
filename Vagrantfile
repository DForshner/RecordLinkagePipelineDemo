# -*- mode: ruby -*-
# vi: set ft=ruby :

Vagrant.configure(2) do |config|

  # Configure box
  config.vm.box = "ubuntu/trusty64"

  # Provider-specific configuration
  config.vm.provider "virtualbox" do |vb|

    # Customize the amount of memory on the VM
    vb.memory = "1024"

  end

  # Provisioning
  config.vm.provision "shell", path: "vagrant/provision.bash"

end