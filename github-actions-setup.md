# GitHub Actions 自动构建 Docker 镜像到阿里云容器制品库

本项目已配置 GitHub Actions 工作流，可以自动构建 Docker 镜像并推送到阿里云容器制品库。

## 配置步骤

### 1. 在 GitHub 仓库中设置 Secrets

进入你的 GitHub 仓库，点击 `Settings` -> `Secrets and variables` -> `Actions`，添加以下 Secrets：

| Secret 名称 | 描述 | 示例值 |
|------------|------|--------|
| `ALIYUN_REGISTRY_URL` | 阿里云容器制品库地址 | `registry.cn-hangzhou.aliyuncs.com` |
| `ALIYUN_REGISTRY_USERNAME` | 阿里云容器制品库用户名 | `your-username` |
| `ALIYUN_REGISTRY_PASSWORD` | 阿里云容器制品库密码 | `your-password` |
| `ALIYUN_NAMESPACE` | 阿里云容器制品库命名空间 | `your-namespace` |

### 2. 阿里云容器制品库配置

#### 获取制品库信息
1. 登录 [阿里云容器镜像服务控制台](https://cr.console.aliyun.com/)
2. 选择个人实例或企业实例
3. 在左侧菜单选择 `镜像仓库`
4. 创建命名空间（如果还没有）
5. 创建镜像仓库

#### 获取访问凭证
- **Registry URL**: 在控制台首页可以看到，格式如 `registry.cn-hangzhou.aliyuncs.com`
- **用户名**: 阿里云账号的用户名
- **密码**: 可以使用阿里云账号密码，或者设置独立的容器镜像服务密码

### 3. 工作流触发条件

当前配置的触发条件：
- 推送到 `main` 或 `master` 分支
- 创建以 `v` 开头的标签（如 `v1.0.0`）
- 向 `main` 或 `master` 分支提交 Pull Request

### 4. 镜像标签策略

- **分支推送**: 使用分支名作为标签（如 `main`, `develop`）
- **PR**: 使用 `pr-{number}` 格式（如 `pr-123`）
- **版本标签**: 支持语义化版本标签
  - `v1.2.3` -> `1.2.3`, `1.2`, `1`, `latest`
- **默认分支**: 额外添加 `latest` 标签

### 5. 多架构支持

工作流配置了多架构构建：
- `linux/amd64` (x86_64)
- `linux/arm64` (ARM64)

### 6. 使用示例

#### 推送新版本
```bash
# 创建并推送版本标签
git tag v1.0.0
git push origin v1.0.0
```

#### 拉取构建的镜像
```bash
# 拉取最新版本
docker pull registry.cn-hangzhou.aliyuncs.com/your-namespace/gameframex-grafana-lokipush:latest

# 拉取特定版本
docker pull registry.cn-hangzhou.aliyuncs.com/your-namespace/gameframex-grafana-lokipush:v1.0.0
```

### 7. 本地测试

在推送到 GitHub 之前，可以本地测试 Docker 构建：

```bash
# 构建镜像
docker build -t gameframex-grafana-lokipush:test .

# 运行测试
docker run --rm -p 5000:5000 gameframex-grafana-lokipush:test
```

### 8. 故障排除

#### 常见问题

1. **认证失败**
   - 检查 Secrets 配置是否正确
   - 确认阿里云账号权限
   - 验证密码是否正确

2. **推送失败**
   - 确认命名空间和仓库名称正确
   - 检查网络连接
   - 查看 Actions 日志详细错误信息

3. **构建失败**
   - 检查 Dockerfile 语法
   - 确认项目文件结构
   - 查看构建日志

#### 查看构建日志

1. 进入 GitHub 仓库
2. 点击 `Actions` 标签
3. 选择对应的工作流运行
4. 查看详细日志

### 9. 高级配置

#### 自定义镜像名称

修改 `.github/workflows/docker-build.yml` 中的 `IMAGE_NAME` 环境变量：

```yaml
env:
  IMAGE_NAME: your-custom-image-name
```

#### 添加构建参数

在 `docker/build-push-action` 步骤中添加 `build-args`：

```yaml
- name: Build and push Docker image
  uses: docker/build-push-action@v5
  with:
    context: .
    file: ./Dockerfile
    push: true
    tags: ${{ steps.meta.outputs.tags }}
    labels: ${{ steps.meta.outputs.labels }}
    build-args: |
      BUILD_VERSION=${{ github.ref_name }}
      BUILD_DATE=${{ github.event.head_commit.timestamp }}
```

#### 条件构建

只在特定条件下构建：

```yaml
- name: Build and push Docker image
  if: github.event_name != 'pull_request'
  uses: docker/build-push-action@v5
  # ... 其他配置
```

## 安全注意事项

1. **不要在代码中硬编码敏感信息**
2. **定期更新 Secrets 中的密码**
3. **使用最小权限原则配置阿里云账号**
4. **定期检查 Actions 运行日志**
5. **考虑使用阿里云 RAM 子账号而非主账号**