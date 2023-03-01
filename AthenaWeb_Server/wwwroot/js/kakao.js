window.kakaoLogin = async () => {
    try {
        //카카오 초기화
        if (!Kakao.isInitialized()) {
            Kakao.init('8743aa8ae98410121d5eb950d8d9fe56');
        }

        //카카오 로그인
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

        //카카오 로그인 정보 가져오기
        // let response = await new Promise((resolve, reject) => {
        //     Kakao.API.request({
        //         url: '/v2/user/me',
        //         success: function (response) {
        //             resolve(response);
        //         },
        //         fail: function (error) {
        //             reject(error);
        //         },
        //     })
        // });

        //카카오 로그인 정보 출력
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
            //카카오 로그아웃
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
            //카카오 토큰 삭제
            Kakao.Auth.setAccessToken(undefined);
        }

        return false;
    } catch (error) {
        console.log(error);
        return true;
    }
}

window.sendKakao = () => {
    Kakao.API.request({
        url: '/v2/api/talk/memo/default/send',
        data: {
          template_object: {
            object_type: 'feed',
            content: {
              title: '딸기 치즈 케익',
              description: '#케익 #딸기 #삼평동 #카페 #분위기 #소개팅',
              image_url:
                'https://mud-kage.kakao.com/dn/Q2iNx/btqgeRgV54P/VLdBs9cvyn8BJXB3o7N8UK/kakaolink40_original.png',
              link: {
                web_url: 'https://developers.kakao.com',
                mobile_web_url: 'https://developers.kakao.com',
              },
            },
            item_content: {
              profile_text: 'Kakao',
              profile_image_url: 'https://mud-kage.kakao.com/dn/Q2iNx/btqgeRgV54P/VLdBs9cvyn8BJXB3o7N8UK/kakaolink40_original.png',
              title_image_url: 'https://mud-kage.kakao.com/dn/Q2iNx/btqgeRgV54P/VLdBs9cvyn8BJXB3o7N8UK/kakaolink40_original.png',
              title_image_text: 'Cheese cake',
              title_image_category: 'Cake',
              items: [
                {
                  item: 'Cake1',
                  item_op: '1000원',
                },
                {
                  item: 'Cake2',
                  item_op: '2000원',
                },
                {
                  item: 'Cake3',
                  item_op: '3000원',
                },
                {
                  item: 'Cake4',
                  item_op: '4000원',
                },
                {
                  item: 'Cake5',
                  item_op: '5000원',
                },
              ],
              sum: 'Total',
              sum_op: '15000원',
            },
            social: {
              like_count: 100,
              comment_count: 200,
            },
            buttons: [
              {
                title: '웹으로 보기',
                link: {
                  mobile_web_url: 'https://developers.kakao.com',
                  web_url: 'https://developers.kakao.com',
                },
              },
              {
                title: '앱으로 보기',
                link: {
                  mobile_web_url: 'https://developers.kakao.com',
                  web_url: 'https://developers.kakao.com',
                },
              },
            ],
          },
        },
      })
        .then(function(response) {
          console.log(response);
        })
        .catch(function(error) {
          console.log(error);
        });
}


class SupportKakao {
    //생성자
    constructor() {
        if(!this.Kakao.isInitialized()){
            this.Kakao.init('8743aa8ae98410121d5eb950d8d9fe56');
        }
    }

    //카카오 로그인
    async login() {
        try {
            await new Promise((resolve, reject) => {
                this.Kakao.Auth.login({
                    success: function (response) {
                        resolve();
                    },
                    fail: function (error) {
                        reject(error);
                    },
                })
            });
            return true;
        } catch (error) {
            console.log(error);
            return false;
        }
    }

    //카카오 로그아웃
    async logout() {
        try {
            if (this.Kakao.Auth.getAccessToken()) {
                await new Promise((resolve, reject) => {
                    this.Kakao.API.request({
                        url: '/v1/user/unlink',
                        success: function (response) {
                            resolve(response);
                        },
                        fail: function (error) {
                            reject(error);
                        },
                    })
                });
                this.Kakao.Auth.setAccessToken(undefined);
            }
            return false;
        } catch (error) {
            console.log(error);
            return true;
        }
    }
}