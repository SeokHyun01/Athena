window.kakaoLogin = async () => {
    _supportKakao = new SupportKakao();
   const answer = _supportKakao.login();
    return answer;
}

let _supportKakao;

window.kakaoLogout = async () => {
   const answer = _supportKakao.logout();
    return answer;
}

window.sendKakao = () => {
    _supportKakao.send();
}


class SupportKakao {
    //생성자
    constructor() {
        if(!Kakao.isInitialized()){
            Kakao.init('8743aa8ae98410121d5eb950d8d9fe56');
        }
    }

    //카카오 로그인
    async login() {
        try {
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

    //카카오 로그아웃
    async logout() {
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

    //카카오 메시지 보내기
    send() {
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
}