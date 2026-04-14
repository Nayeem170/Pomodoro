window.localDateTime = {
    getLocalDate: function() {
        const now = new Date();
        return new Date(now.getFullYear(), now.getMonth(), now.getDate());
    },
    getLocalDateTime: function() {
        return new Date();
    },
    getTimezoneOffset: function() {
        return new Date().getTimezoneOffset();
    }
};
