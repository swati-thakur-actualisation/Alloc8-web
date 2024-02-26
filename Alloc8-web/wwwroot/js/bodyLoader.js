var bodyLoader = {
    template: '<div class="body_loader_backdrop"><div class="body_loader_container"><div class="loader"></div></div></div>',
    add: function ($container) {
        $container = $($container);

        dd('loader.add', $container);

        if ($('.loading', $container).length)
            return;

        $container.css('position', 'relative');
        $container.append(bodyLoader.template);
    },
    remove: function ($container) {
        $container = $($container);
        dd('loader.remove', $container);
        $('.body_loader_backdrop', $container).remove();
    }
};