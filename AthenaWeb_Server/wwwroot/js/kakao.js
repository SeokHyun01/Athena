window.kakaoLogin = async () => {
    try {
        //카카오 로그인이 처음이라면
        if (!Kakao.isInitialized()) {
            Kakao.init('8743aa8ae98410121d5eb950d8d9fe56');
        }

        await new Promise((resolve, reject) => {
            Kakao.Auth.login({
                success: function (response) {
                    resolve();
                },
                fail: function (error) {
                    reject(error);
                },
            })
        });

        let response = await new Promise((resolve, reject) => {
            Kakao.API.request({
                url: '/v2/user/me',
                success: function (response) {
                    resolve(response);
                },
                fail: function (error) {
                    reject(error);
                },
            })
        });

        console.log(response.id, response.kakao_account.profile.nickname);
        return true;
    } catch (error) {
        console.log(error);
        return false;
    }
}


window.kakaoLogout = async () => {
    try {
        if (Kakao.Auth.getAccessToken()) {
            await new Promise((resolve, reject) => {
                Kakao.API.request({
                    url: '/v1/user/unlink',
                    success: function (response) {
                        resolve(response);
                    },
                    fail: function (error) {
                        reject(error);
                    },
                })
            });
            Kakao.Auth.setAccessToken(undefined);
        }

        return false;
    } catch (error) {
        console.log(error);
        return true;
    }
}
