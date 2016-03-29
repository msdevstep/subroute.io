define(['jquery', 'services/authentication'], function($, auth) {
    return new function() {
        var self = this;

        self.lock = new window.Auth0Lock('NuIcxXuxDMf2xZkP0kTX0aa28RyoFvAJ', 'subroute.auth0.com');

        self.request = function (options) {
            var deferred = $.Deferred();

            $.ajax(options).fail(function (err) {
                // Attempt to get a new token using the refresh token if the call was unauthorized.
                if (err.status === 401) {
                    // Determine if we have a refresh token in local storage, if we don't then
                    // reject the deferred using the original ajax error value.
                    var refreshToken = localStorage.getItem('refresh_token');
                    if (!refreshToken) {
                        // Could not refresh, can't possibly renew token.
                        deferred.reject(err);
                        return;
                    };

                    // We have a refresh token, so use it to request a new token.
                    self.lock.getClient().refreshToken(refreshToken, function(refreshError, delegationResult) {
                        // Reject the deferred if we were unable to get a new token issued.
                        if (refreshError) {
                            // Could not refresh, log the user out.
                            auth.logout();
                            deferred.reject(err);
                            return;
                        };

                        // We received a new token, woo!, store them in local storage.
                        localStorage.setItem('id_token', delegationResult.id_token);

                        // Reinitialize auth with new token.
                        auth.initialize();

                        // Now that we have a valid token, make the original request
                        // again and return it's promise information.
                        $.ajax(options).then(function(response) {
                            deferred.resolve(response);
                        }).fail(function(error) {
                            deferred.reject(error);
                        });
                    });
                } else {
                    deferred.reject(err);
                };
            }).then(function(response) {
                deferred.resolve(response);
            });

            // Return our deferred object as a valid promise.
            return deferred.promise();
        };
    };
});